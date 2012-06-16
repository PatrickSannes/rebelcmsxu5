using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

using Umbraco.Framework.DataManagement;
using Umbraco.Framework.DataManagement.Linq;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport;
using File = Umbraco.Framework.Persistence.Model.IO.File;

namespace Umbraco.Framework.IO
{
    public class RepositoryReadWriter : DisposableObject, IRepositoryReadWriter
    {
        private readonly DataContext _dataContext;
        private readonly string _supportedExtensions;
        private readonly string _rootFolder;
        private readonly string _relationsFolder;
        private readonly DirectoryInfo _directory;

        public RepositoryReadWriter(DataContext dataContext)
        {
            _dataContext = dataContext;
            _supportedExtensions = _dataContext.SupportedExtensions;
            _rootFolder = _dataContext.RootPath.TrimEnd('\\');
            _relationsFolder = Path.Combine(_rootFolder, "Relations");
            _directory = new DirectoryInfo(_rootFolder);
        }

        internal HiveId GenerateId(string path)
        {
            //TODO: "storage" is hard coded, this needs to somehow come from config, better yet the entire ID generation for Hive providers should happen in a base class.

            //get the path value
            var idVal = path.Replace(_rootFolder, string.Empty).Replace("/", @"\");
            if (idVal.IsNullOrWhiteSpace())
            {
                //if the path value is empty, it means we're on the root, and we MUST return a '/' or else the hive id rules don't match for root nodes
                idVal = "/";
            }

            var id = new HiveId("storage", _dataContext.HiveProvider.ProviderAlias, new HiveIdValue(idVal));

            return id;
        }

        private File Hydrate(FileSystemInfo fileInfo)
        {
            var id = GenerateId(fileInfo.FullName);

            var file = new File(id)
            {
                Name = fileInfo.Name,
                Location = fileInfo.FullName,
                IsContainer = fileInfo is DirectoryInfo,
                UtcCreated = fileInfo.CreationTimeUtc,
                UtcModified = fileInfo.LastWriteTimeUtc,
                AbsolutePath = _dataContext.ApplicationRelativeRoot + (string)id.Value
            };

            if (!file.IsContainer)
            {
                //assign the lazy load delegate to access the stream
                file.LazyContentStream = new Lazy<Stream>(() => ((FileInfo)fileInfo).OpenRead());                
            }

            file.Relations.LazyLoadFactory =
                (source, scope) =>
                {
                    var sourceAsFile = (File)source;
                    var relations = new List<Relation>();

                    switch (scope)
                    {
                        case HierarchyScope.AllOrNone:
                            relations.AddRange(GetParent(sourceAsFile));
                            relations.AddRange(GetChildren(file, fileInfo));
                            break;
                        case HierarchyScope.Children:
                        case HierarchyScope.Descendents:
                        case HierarchyScope.DescendentsOrSelf:
                            relations.AddRange(GetChildren(file, fileInfo));
                            break;
                        case HierarchyScope.Parent:
                        case HierarchyScope.Parents:
                        case HierarchyScope.Ancestors:
                        case HierarchyScope.AncestorsOrSelf:
                            relations.AddRange(GetParent(sourceAsFile));
                            break;
                    }

                    return relations;
                };

            return file;
        }

        private IEnumerable<Relation> GetParent(File file)
        {
            var relations = new List<Relation>();

            // Get parent file relation
            var currentFolder = (file.IsContainer
                                     ? file.Location
                                     : file.Location.Replace(file.Name, string.Empty))
                                     .TrimEnd(Path.DirectorySeparatorChar);

            if (currentFolder != _rootFolder)
            {
                var dir = new DirectoryInfo(currentFolder);
                relations.Add(new Relation(
                    FixedRelationTypes.FileRelationType,
                    file.IsContainer
                        ? GetEntity<File>(GenerateId(dir.Parent.FullName + Path.DirectorySeparatorChar))
                        : GetEntity<File>(GenerateId(dir.FullName)),
                    file
                ));
            }
            else
            {
                //if the folder matches the root folder and the file is not a container, then we need to return the root folder
                if (!file.IsContainer)
                {
                    relations.Add(new Relation(
                       FixedRelationTypes.FileRelationType,
                       Hydrate(_directory),
                       file
                   ));   
                }                
            }
  
            // Get other relations
            relations.AddRange(GetRelationsByPattern("*-" + file.Id.ToString().ToMd5()));

            return relations;
        }

        private IEnumerable<Relation> GetChildren(File file, FileSystemInfo fileInfo)
        {
            var relations = new List<Relation>();

            // Get file relations
            if (file.IsContainer)
            {
                var files = GetFiles((DirectoryInfo) fileInfo);

                relations.AddRange(files.Select(f =>
                    new Relation(
                            FixedRelationTypes.FileRelationType,
                            file,
                            Hydrate(f)
                        )
                    )
                    .ToList());
            }

            // Get other relations
            relations.AddRange(GetRelationsByPattern(file.Id.ToString().ToMd5() + "-*"));

            return relations;
        }

        protected override void DisposeResources()
        {

        }

        public IQueryContext<TypedEntity> QueryContext
        {
            get { throw new NotImplementedException(); }
        }

        public T GetEntity<T>(HiveId id) where T : AbstractEntity
        {
            var file = GetFile(id);

            if (file == null)
                return null;

            return Hydrate(file) as T;
        }

        public IEnumerable<T> GetEntityByRelationType<T>(AbstractRelationType relationType, HiveId sourceId, params RelationMetaDatum[] metaDatum)
            where T : class, IRelatableEntity
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetEntities<T>() where T : AbstractEntity
        {
            //TODO: Should the found files be cached? --Aaron
            var files = GetFiles(_directory);

            return files
                .Select(Hydrate)
                .OrderBy(file => file.Name)
                .Cast<T>();
        }

        public bool Exists<T>(HiveId id) where T : AbstractEntity
        {
            var file = GetFile(id);

            return file != null;
        }

        public RevisionCollection<T> GetRevisions<T>(HiveId entityId, RevisionStatusType revisionStatusType = null) where T : TypedEntity
        {
            throw new NotSupportedException("File system does not support history");
        }

        public Revision<T> GetRevision<T>(HiveId entityId, HiveId revisionId) where T : TypedEntity
        {
            throw new NotSupportedException("File system does not support history");
        }

        public EntitySnapshot<T> GetEntitySnapshot<T>(HiveId entityUri, HiveId revisionId) where T : TypedEntity
        {
            throw new NotImplementedException();
        }

        public EntitySnapshot<T> GetLatestSnapshot<T>(HiveId entityUri, RevisionStatusType revisionStatusType = null) where T : TypedEntity
        {
            throw new NotImplementedException();
        }

        public T GetByPath<T>(HiveId path, AbstractRelationType relationType = null, RevisionStatusType statusType = null) where T : TypedEntity
        {
            Mandate.ParameterNotEmpty(path, "path");

            return GetEntity<T>(path);
        }

        public void AddOrUpdate(AbstractEntity persistedEntity)
        {
            Mandate.That<ArgumentException>(typeof(File).IsAssignableFrom(persistedEntity.GetType()));
            var file = (File)persistedEntity;
            Mandate.That<ArgumentNullException>(!file.Name.IsNullOrWhiteSpace() || !file.Location.IsNullOrWhiteSpace());

            // Make sure that the incoming location has the root storage area prefixed if necessary
            file.Location = EnsureLocationRooted(string.IsNullOrEmpty(file.Location) ? file.Name : file.Location);

            //ensure the file name is set which should always be based on the location
            file.Name = Path.GetFileName(file.Location);

            // Ensure we have an id for the file
            if (HiveIdExtensions.IsNullValueOrEmpty(file.Id))
            {
                file.Id = GenerateId(file.Location);
            }

            // Ensure that the folder exists, if this item is a folder)
            if (file.IsContainer)
            {
                var dir = new DirectoryInfo(file.Location);
                if (!dir.Exists)
                    dir.Create();
            }
            else
            {
                var containerPath = Path.GetDirectoryName(file.Location);
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
                var physicalForWriting = new FileInfo(file.Location);
                if(physicalForWriting.Exists)
                {
                    physicalForWriting.Delete();
                }
                using (var writer = physicalForWriting.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                {
                    writer.Write(file.ContentBytes, 0, file.ContentBytes.Length);
                }
            }

            //var physicalFile = new FileInfo(file.Location);
            //if (physicalFile.Name != file.Name)
            //{
            //    physicalFile.MoveTo(Path.Combine(physicalFile.Directory.FullName, file.Name));
            //}

            // Write relations to disc
            foreach(var relation in file.Relations)
            {
                if (relation.Type.RelationName != FixedRelationTypes.FileRelationType.RelationName)
                {
                    EnsureRelationsFolderExists();

                    var sourceMd5 = relation.SourceId.ToString().ToMd5();
                    var destMd5 = relation.DestinationId.ToString().ToMd5();
                    var relationPath = Path.Combine(_relationsFolder, sourceMd5 + "-" + destMd5 + ".xml");
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
        }

        public void Delete<T>(HiveId entityId)
        {
            Mandate.ParameterNotEmpty(entityId, "entityId");

            var file = GetEntity<File>(entityId);

            if (file == null)
            {
                throw new IOException(
                    string.Format("File with id '{0}' does not exist in this providers storage location", entityId));
            }

            if (!file.IsContainer)
                System.IO.File.Delete(file.Location);
            else
                System.IO.Directory.Delete(file.Location, true);

            // Delete any relations
            var entityMd5 = entityId.ToString().ToMd5();
            var searchPattern = "*" + entityMd5 + "*.xml";
            if (Directory.Exists(_relationsFolder))
            {
                var files = Directory.GetFiles(_relationsFolder, searchPattern);
                foreach (var filePath in files)
                {
                    System.IO.File.Delete(filePath);
                }    
            }
            
        }

        public void AddOrUpdate<T>(Revision<T> revision) where T : TypedEntity
        {
            throw new NotSupportedException("File system does not support revisions");
        }

        private void EnsureRelationsFolderExists()
        {
            if (!System.IO.Directory.Exists(_relationsFolder))
                System.IO.Directory.CreateDirectory(_relationsFolder);
        }

        private string EnsureLocationRooted(string location)
        {
            if (location == null) return string.Empty;
            if (location.StartsWith("\\", StringComparison.InvariantCultureIgnoreCase) || (!location.StartsWith(_rootFolder, StringComparison.InvariantCultureIgnoreCase)))
                return _rootFolder + "\\" + location.TrimStart('\\');
            return location;
        }

        //TODO: We need to add an internal cache to this lookup as it gets called quite a few times when dealing with relations
        private IEnumerable<FileSystemInfo> GetFiles(DirectoryInfo fileInfo)
        {
            //Console.WriteLine("GetFiles " + fileInfo.Name);

            var files = new List<FileSystemInfo>();
            foreach (var extension in _supportedExtensions.Split(';'))
                files.AddRange(fileInfo.GetFiles(extension, SearchOption.TopDirectoryOnly));

            files.AddRange(fileInfo.GetDirectories("*", SearchOption.TopDirectoryOnly));
            return files;
        }

        //TODO: We need to add an internal cache to this lookup as it gets called quite a few times when dealing with relations
        private FileSystemInfo GetFile(HiveId id)
        {
            //Console.WriteLine("GetFile " + id.ToString(HiveIdFormatStyle.AsUri));

            //if there is an empty id, then we need to return our root
            if (!id.IsNullValueOrEmpty() && (((string)id.Value.Value).IsNullOrWhiteSpace() || ((string)id.Value.Value) == "/") || ((string)id.Value.Value) == "\\")
                return _directory;

            //NOTE: If we don't trim the trailing '\\' we will end up in an infinite loop with the logic
            // below because with '\\' on the end it will find all files and the folder, whereas we want to 
            // only find the file or the folder.
            // We also need to trim the start because it can't start with \\ otherwise the system thinks were querying a UNC path.
            var stringId = ((string) id.Value.Value).TrimEnd(Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);

            var file = _directory
                .GetFiles(stringId, SearchOption.TopDirectoryOnly)
                .OfType<FileSystemInfo>()
                .Concat(_directory.GetDirectories(stringId, SearchOption.TopDirectoryOnly))
                .FirstOrDefault();
            return file;
        }

        private IEnumerable<Relation> GetRelationsByPattern(string searchPattern)
        {
            var relations = new List<Relation>();

            if (Directory.Exists(_relationsFolder))
            {
                var relationFilePaths = System.IO.Directory.GetFiles(_relationsFolder, searchPattern + ".xml");
                foreach (var relationFilePath in relationFilePaths)
                {
                    var relationFileInfo = new FileInfo(relationFilePath);
                    var relationFileContent = "";
                    using (var reader = relationFileInfo.OpenRead())
                    {
                        var relationFileBytes = new byte[reader.Length];
                        reader.Read(relationFileBytes, 0, (int) reader.Length);
                        relationFileContent = Encoding.UTF8.GetString(relationFileBytes);
                    }
                    var relation = RelationSerializer.FromXml(relationFileContent);
                    relation.Source = GetEntity<File>(relation.SourceId); // TODO: Might not be a file?
                    relation.Destination = GetEntity<File>(relation.DestinationId); // TODO: Might not be a file?
                    relations.Add(relation);
                }
            }

            return relations;
        }
    }
}
