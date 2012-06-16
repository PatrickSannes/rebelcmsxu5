using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderSupport;
using File = Umbraco.Framework.Persistence.Model.IO.File;
using Umbraco.Hive;

namespace Umbraco.Hive.Providers.IO
{
    using Umbraco.Framework.Linq.QueryModel;

    using Umbraco.Framework.Linq.ResultBinding;

    public class EntityRepository : AbstractEntityRepository
    {
        public EntityRepository(ProviderMetadata providerMetadata, AbstractSchemaRepository schemas, AbstractRevisionRepository<TypedEntity> revisions, IProviderTransaction providerTransaction, Settings settings, IFrameworkContext frameworkContext)
            : base(providerMetadata, providerTransaction, revisions, schemas, frameworkContext)
        {
            Settings = settings;

            EnsureRootPath();
        }

        protected Settings Settings { get; set; }

        public EntityRepository(ProviderMetadata providerMetadata, Settings settings, IFrameworkContext frameworkContext)
            : base(providerMetadata, frameworkContext)
        {
            Settings = settings;

            EnsureRootPath();
        }


        internal HiveId NormaliseId(HiveId path)
        {
            Mandate.ParameterCondition(path.Value.Type == HiveIdValueTypes.String, "id");
            var pathValue = NormaliseValue(path.Value);
            return new HiveId(ProviderMetadata.MappingRoot, ProviderMetadata.Alias, pathValue);
        }

        internal HiveIdValue NormaliseValue(HiveIdValue value)
        {
            Mandate.ParameterCondition(value.Type == HiveIdValueTypes.String, "id");

            var pathValue = (string)value.Value;

            pathValue = pathValue.Replace("/", "\\");

            if (!pathValue.IsNullOrWhiteSpace() && pathValue != "\\")
            {
                if (!pathValue.StartsWith(Settings.AbsoluteRootedPath))
                    return new HiveIdValue(pathValue);
            }

            pathValue = pathValue.Replace(Settings.AbsoluteRootedPath, string.Empty);

            return new HiveIdValue(pathValue);
        }

        internal void EnsureRootPath()
        {
            if (!Directory.Exists(Settings.AbsoluteRootedPath))
                Directory.CreateDirectory(Settings.AbsoluteRootedPath);
        }

        public HiveId GenerateId(string path)
        {
            return new HiveId(
                ProviderMetadata.MappingRoot,
                ProviderMetadata.Alias,
                NormaliseValue(new HiveIdValue(path.Replace(Settings.AbsoluteRootedPath.NormaliseDirectoryPath(), string.Empty))));
        }

        private File Hydrate(FileSystemInfo fileInfo)
        {
            var id = GenerateId(fileInfo.FullName);

            var file = new File(id)
            {
                Name = fileInfo.Name,
                RootedPath = fileInfo.FullName,
                IsContainer = fileInfo is DirectoryInfo,
                UtcCreated = fileInfo.CreationTimeUtc,
                UtcModified = fileInfo.LastWriteTimeUtc,
                UtcStatusChanged = fileInfo.LastWriteTimeUtc,
                RootRelativePath = Settings.ApplicationRelativeRoot + ((string)id.Value).Replace('\\', '/'),
            };

            file.PublicUrl = Settings.RootPublicDomain.TrimEnd('/') + "/" + file.RootRelativePath.TrimStart('~', '/');

            if (!file.IsContainer)
            {
                //assign the lazy load delegate to access the stream
                file.SetContentStreamFactory(theFile => ((FileInfo)fileInfo).OpenRead());
            }

            return file;
        }

        internal string GetContainingFolder(File file)
        {
            var dir = Path.GetDirectoryName(file.RootedPath);

            // Ensure the folder name ends with a trailing slash)
            return dir.NormaliseDirectoryPath();
        }

        internal File GetParentModel(File file)
        {
            //check if the entity is actually the root path, if so, then return null
            if (file.IsContainer && file.RootedPath.NormaliseDirectoryPath().InvariantEquals(Settings.AbsoluteRootedPath.NormaliseDirectoryPath()))
            {
                //we are the root folder, return null
                return null;
            }

            var parent = GetParentDirectoryInfo(file);
            if (parent == null)
                return null;

            var normalisedPath = parent.FullName.NormaliseDirectoryPath();            
            return this.Get<File>(GenerateId(normalisedPath));
        }

        internal IEnumerable<Relation> GetParentRelations(HiveId childId)
        {
            var child = this.Get<File>(childId);

            // Check if we're already at the root, and return nothing
            var childLocationNormalised = child.RootedPath.NormaliseDirectoryPath();
            var rootNormalised = Settings.AbsoluteRootedPath.NormaliseDirectoryPath();
            if (childLocationNormalised.InvariantEquals(rootNormalised))
                yield break;

            var parentModel = GetParentModel(child);
            if (parentModel != null)
                yield return new Relation(FixedRelationTypes.DefaultRelationType, parentModel, child);

            // Get other relations
            foreach (var relation in GetRelationsByPattern("*-" + child.Id.Value.ToString().ToMd5()))
            {
                yield return relation;
            }
        }

        private static DirectoryInfo GetParentDirectoryInfo(File file)
        {
            DirectoryInfo parent = null;

            // If the file is a container, the parent is the parent folder
            if (file.IsContainer)
            {
                //check if the current object is the root path, if so return null
                if (((string)file.Id.Value.Value).IsNullOrWhiteSpace())
                {
                    return null;
                }
                parent = Directory.GetParent(file.RootedPath.TrimEnd('\\')); // .NET thinks the "current" folder in a path is the one without a trailing slash
            }
            else
            {
                parent = Directory.GetParent(file.RootedPath);
            }
            return parent;
        }

        internal IEnumerable<Relation> GetRelationsByPattern(string searchPattern)
        {
            var relations = new List<Relation>();

            if (Directory.Exists(Settings.RelationsStoragePath))
            {
                var relationFilePaths = System.IO.Directory.GetFiles(Settings.RelationsStoragePath, searchPattern + ".xml");
                foreach (var relationFilePath in relationFilePaths)
                {
                    var relationFileInfo = new FileInfo(relationFilePath);
                    var relationFileContent = "";
                    using (var reader = relationFileInfo.OpenRead())
                    {
                        var relationFileBytes = new byte[reader.Length];
                        reader.Read(relationFileBytes, 0, (int)reader.Length);
                        relationFileContent = Encoding.UTF8.GetString(relationFileBytes);
                    }
                    var relationById = RelationSerializer.FromXml(relationFileContent);
                    // TODO: The source / destination of this relation might not be a file in the future but for the moment that's all this provider supports
                    var relation = new Relation(relationById.Type, 
                        this.Get<File>(relationById.SourceId),
                        this.Get<File>(relationById.DestinationId),
                        relationById.Ordinal,
                        relationById.MetaData.ToArray());
                    relations.Add(relation);
                }
            }

            return relations;
        }

        private IEnumerable<Relation> GetChildRelations(HiveId parentId, FileSystemInfo fileInfo)
        {
            var relations = new List<Relation>();

            var parent = this.Get<File>(parentId);

            // Get file relations
            if (parent.IsContainer)
            {
                var files = GetFiles((DirectoryInfo)fileInfo);

                relations.AddRange(files.Select(f =>
                    new Relation(
                            FixedRelationTypes.DefaultRelationType,
                            parent,
                            Hydrate(f)
                        )
                    )
                    .ToList());
            }

            // Get other relations
            relations.AddRange(GetRelationsByPattern(parent.Id.Value.ToString().ToMd5() + "-*"));

            return relations;
        }

        //TODO: We need to add an internal cache to this lookup as it gets called quite a few times when dealing with relations
        private IEnumerable<FileSystemInfo> GetFiles(DirectoryInfo fileInfo)
        {
            var files = new List<FileSystemInfo>();
            foreach (var extension in Settings.SupportedExtensions.Split(';'))
                files.AddRange(fileInfo.GetFiles(extension, SearchOption.TopDirectoryOnly));

            var directories = fileInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
            files.AddRange(directories.Where(x => !Settings.ExcludedDirectories.Split(';').Aggregate(false, (current, pattern) => current || new WildcardRegex(pattern).IsMatch(x.Name))));

            return files;
        }

        //TODO: We need to add an internal cache to this lookup as it gets called quite a few times when dealing with relations
        private FileSystemInfo GetFile(HiveId id)
        {
            Mandate.ParameterCondition(id.Value.Type == HiveIdValueTypes.String, "id");
            var directory = new DirectoryInfo(Settings.AbsoluteRootedPath);

            //if there is an empty id, then we need to return our root
            //empty id could also be a null string
            if (id == HiveId.Empty || (id.Value.Type == HiveIdValueTypes.String && ((string)id.Value).IsNullOrWhiteSpace()))
                return directory;

            //the value should not begin/end with a DirectorySeperatorChar and should contain '\' instead of '/'
            var path = ((string)id.Value.Value)
                .Replace("/", "\\")
                .TrimEnd(Path.DirectorySeparatorChar)
                .TrimStart(Path.DirectorySeparatorChar);
            var combinedPath = Path.Combine(Settings.AbsoluteRootedPath, path);
            if (Directory.Exists(combinedPath.NormaliseDirectoryPath()))
                return new DirectoryInfo(combinedPath);
            if (System.IO.File.Exists(combinedPath))
                return new FileInfo(combinedPath);

            return null;
        }

        protected override void DisposeResources()
        {
            return;
        }

        public override IEnumerable<T> PerformGet<T>(bool allOrNothing, params HiveId[] ids)
        {
            // Establish an output list in case allOrNothing is true
            // If it's not true, we'll yield straight away, otherwise we'll
            // add to a list first in order to check that every item matched an id
            // before then returning an iterator to the total list
            var output = new List<T>();
            foreach (var hiveId in ids)
            {
                var id = NormaliseId(hiveId);
                var file = GetFile(id);

                if (file == null)
                    if (allOrNothing) break; else yield break;

                //TODO: Perhaps we should throw an exception if this cast doesn't work? otherwise if someone passes in something other than file there'll be issues.
                var hydrated = Hydrate(file) as T;
                if (allOrNothing) output.Add(hydrated); else yield return hydrated;
            }
            if (allOrNothing)
            {
                if (output.Count() != ids.Length) yield break;
                foreach (var hydrated in output)
                {
                    yield return hydrated;
                }
            }
        }

        public override IEnumerable<T> PerformExecuteMany<T>(QueryDescription query, ObjectBinder objectBinder) { throw new NotImplementedException(); }

        public override T PerformExecuteScalar<T>(QueryDescription query, ObjectBinder objectBinder) { throw new NotImplementedException(); }

        public override T PerformExecuteSingle<T>(QueryDescription query, ObjectBinder objectBinder) { throw new NotImplementedException(); }

        public override IEnumerable<T> PerformGetAll<T>()
        {
            var files = GetFiles(new DirectoryInfo(Settings.AbsoluteRootedPath));

            return files
                .Select(Hydrate)
                .OrderBy(file => file.Name)
                .Cast<T>()
                .AsQueryable();
        }

        public override bool Exists<TEntity>(HiveId id)
        {
            var file = GetFile(id);

            return file != null;
        }

        public override bool CanReadRelations
        {
            get { return true; }
        }

        public override IEnumerable<IRelationById> PerformGetParentRelations(HiveId childId, RelationType relationType = null)
        {
            var parents = GetParentRelations(childId)
                .Where(x => relationType == null || x.Type.RelationName == relationType.RelationName);

            return parents;
        }

        public override IRelationById PerformFindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IRelationById> PerformGetAncestorRelations(HiveId descendentId, RelationType relationType = null)
        {
            var firstLevel = GetParentRelations(descendentId, relationType);
            var recursiveAncestors = firstLevel
                .SelectRecursive(x => GetParentRelations(x.SourceId, relationType));
            return recursiveAncestors;
        }

        public override IEnumerable<IRelationById> PerformGetDescendentRelations(HiveId ancestorId, RelationType relationType = null)
        {
            var children = GetChildRelations(ancestorId, relationType);
            var recursiveChildren = children
                .SelectRecursive(x => GetChildRelations(x.DestinationId, relationType));
            //.Reverse();
            return recursiveChildren;
        }

        public override IEnumerable<IRelationById> PerformGetChildRelations(HiveId parentId, RelationType relationType = null)
        {
            var children = GetChildRelations(parentId, GetFile(parentId))
                .Where(x => relationType == null || x.Type.RelationName == relationType.RelationName);

            return children;
        }

        private void EnsureRelationsFolderExists()
        {
            if (!System.IO.Directory.Exists(Settings.RelationsStoragePath))
                System.IO.Directory.CreateDirectory(Settings.RelationsStoragePath);
        }

        private string EnsureLocationRooted(string location)
        {
            if (location == null) return string.Empty;
            if (location.StartsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.InvariantCultureIgnoreCase) || (!location.StartsWith(Settings.AbsoluteRootedPath, StringComparison.InvariantCultureIgnoreCase)))
                return StringExtensions.NormaliseDirectoryPath(Settings.AbsoluteRootedPath) + location.TrimStart(Path.DirectorySeparatorChar);
            return location;
        }

        protected override void PerformAddOrUpdate(TypedEntity persistedEntity)
        {
            Mandate.That<ArgumentException>(typeof(File).IsAssignableFrom(persistedEntity.GetType()));
            var file = (File)persistedEntity;
            Mandate.That<ArgumentNullException>(!file.Name.IsNullOrWhiteSpace() || !file.RootedPath.IsNullOrWhiteSpace());

            // Make sure that the incoming location has the root storage area prefixed if necessary
            file.RootedPath = EnsureLocationRooted(string.IsNullOrEmpty(file.RootedPath) ? file.Name : file.RootedPath);

            //ensure the file name is set which should always be based on the location
            file.Name = Path.GetFileName(file.RootedPath);

            //get a reference to the previous filename from the id before we update it
            var oldFileName = !file.Id.IsNullValueOrEmpty() ? file.Id.Value.ToString() : null;

            // Set the id if it is a new file, or reset it incase the file name has changed
            file.Id = GenerateId(file.RootedPath);

            // Ensure that the folder exists, if this item is a folder)
            if (file.IsContainer)
            {
                var dir = new DirectoryInfo(file.RootedPath);
                if (!dir.Exists)
                    dir.Create();
            }
            else
            {
                var containerPath = Path.GetDirectoryName(file.RootedPath);
                if (containerPath != null)
                {
                    var dir = new DirectoryInfo(containerPath);
                    if (!dir.Exists)
                        dir.Create();
                }
            }

            // Write the file, provided it's not a directory
            if (!file.IsContainer)
            {
                if(!string.IsNullOrWhiteSpace(oldFileName))
                {
                    var oldFilePath = EnsureLocationRooted(oldFileName);
                    var oldFileInfo = new FileInfo(oldFilePath);
                    if(oldFileInfo.Exists)
                        oldFileInfo.Delete();
                }

                var physicalForWriting = new FileInfo(file.RootedPath);
                if (physicalForWriting.Exists)
                {
                    physicalForWriting.Delete();
                }
                using (var writer = physicalForWriting.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                {
                    writer.Write(file.ContentBytes, 0, file.ContentBytes.Length);
                }
            }
        }

        protected override void PerformDelete<T>(HiveId id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            var file = this.Get<File>(id);

            if (file == null)
            {
                throw new IOException(
                    string.Format("File with id '{0}' does not exist in this providers storage location", id));
            }

            if (!file.IsContainer)
                System.IO.File.Delete(file.RootedPath);
            else
                System.IO.Directory.Delete(file.RootedPath, true);

            // Delete any relations
            var entityMd5 = id.Value.ToString().ToMd5();
            var searchPattern = "*" + entityMd5 + "*.xml";
            if (Directory.Exists(Settings.RelationsStoragePath))
            {
                var files = Directory.GetFiles(Settings.RelationsStoragePath, searchPattern);
                foreach (var filePath in files)
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        public override bool CanWriteRelations
        {
            get { return true; }
        }

        protected override void PerformAddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> relation)
        {
            // Write relations to disc
            // TODO: Update to perform in the same manner as the NH provider i.e. in the base class by hooking AddOrUpdate
            if (relation.Type.RelationName != FixedRelationTypes.DefaultRelationType.RelationName)
            {
                EnsureRelationsFolderExists();

                var sourceMd5 = relation.SourceId.Value.ToString().ToMd5();
                var destMd5 = relation.DestinationId.Value.ToString().ToMd5();
                var relationPath = Path.Combine(Settings.RelationsStoragePath, sourceMd5 + "-" + destMd5 + ".xml");
                var relationFileInfo = new FileInfo(relationPath);
                var relationContent = RelationSerializer.ToXml(relation);
                var relationContentBytes = Encoding.UTF8.GetBytes(relationContent.ToString());

                if (relationFileInfo.Exists)
                    relationFileInfo.Delete();

                using (
                    var writer = relationFileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                {
                    writer.Write(relationContentBytes, 0, relationContentBytes.Length);
                }
            }
        }

        protected override void PerformRemoveRelation(IRelationById relation)
        {
            if (relation.Type.RelationName != FixedRelationTypes.DefaultRelationType.RelationName)
            {
                var sourceMd5 = relation.SourceId.Value.ToString().ToMd5();
                var destMd5 = relation.DestinationId.Value.ToString().ToMd5();
                var searchPattern = sourceMd5 + "-" + destMd5 + ".xml";
                if (Directory.Exists(Settings.RelationsStoragePath))
                {
                    var files = Directory.GetFiles(Settings.RelationsStoragePath, searchPattern);
                    foreach (var filePath in files)
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
        }
    }
}
