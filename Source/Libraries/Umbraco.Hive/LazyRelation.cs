using System.Linq;
using System.Threading;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive
{
    public class LazyRelation<TRelatable> : Relation where TRelatable : class, IRelatableEntity
    {
        private ICoreReadonlyRepository<TRelatable> _sourceRepo;
        private ICoreReadonlyRepository<TRelatable> _destinationRepo;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        public LazyRelation(ICoreReadonlyRepository<TRelatable> sourceRepo, AbstractRelationType type, HiveId sourceId, HiveId destinationId, int ordinal, params RelationMetaDatum[] metaData)
            : this(sourceRepo, sourceRepo, type, sourceId, destinationId, ordinal, metaData)
        {
        }

        public LazyRelation(ICoreReadonlyRepository<TRelatable> sourceRepo, ICoreReadonlyRepository<TRelatable> destinationRepo, AbstractRelationType type, HiveId sourceId, HiveId destinationId, int ordinal, params RelationMetaDatum[] metaData)
            : base(type, sourceId, destinationId, ordinal, metaData)
        {
            IsLoaded = false;
            _sourceRepo = sourceRepo;
            _destinationRepo = destinationRepo;
        }

        public bool IsLoaded { get; private set; }

        public override IRelatableEntity Source
        {
            get
            {
                EnsureLoaded();
                return base.Source;
            }
            set
            {
                base.Source = value;
            }
        }

        public override IRelatableEntity Destination
        {
            get
            {
                EnsureLoaded();
                return base.Destination;
            }
            set
            {
                base.Destination = value;
            }
        }

        public void EnsureLoaded()
        {
            if (IsLoaded) return;
            using (new WriteLockDisposable(_locker))
            {
                try
                {
                    // If the source & destination repo are the same, use only one with both ids
                    if (_sourceRepo == _destinationRepo)
                    {
                        var results = _sourceRepo.Get<TRelatable>(true, SourceId, DestinationId).ToArray();

                        // Because both the source and destination repo are the same, it should be safe to just compare the id
                        // value ignoring any differences in group root or provider id
                        base.Source = results.Single(x => x.Id.Value == SourceId.Value);
                        base.Destination = results.Single(x => x.Id.Value == DestinationId.Value);
                    }
                    else
                    {
                        base.Source = _sourceRepo.Get<TRelatable>(SourceId);
                        base.Destination = _destinationRepo.Get<TRelatable>(DestinationId);
                    }
                }
                finally
                {
                    IsLoaded = true;
                }
            }
        }
    }
}
