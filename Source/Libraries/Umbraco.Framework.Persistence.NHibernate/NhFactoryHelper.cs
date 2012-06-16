using System;
using System.Threading;
using NHibernate;
using NHibernate.Context;
using Umbraco.Framework.Context;
using Umbraco.Framework.Diagnostics;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Framework.Persistence.NHibernate
{
    using global::NHibernate.Cfg;
    using global::NHibernate.Engine;

    public class NhFactoryHelper : DisposableObject
    {
        public NhFactoryHelper(Configuration config, ISession singleProvidedSession, bool leaveSessionOpenOnDispose, bool isSingleSessionFinalized, IFrameworkContext frameworkContext)
        {
            Config = config;
            SingleProvidedSession = singleProvidedSession;
            LeaveSessionOpenOnDispose = leaveSessionOpenOnDispose;
            IsSingleSessionFinalized = isSingleSessionFinalized;
            FrameworkContext = frameworkContext;
        }

        public ISession GetSessionFromTransaction(IProviderTransaction transaction, bool isReadonly)
        {
            var nhTransaction = transaction as NhProviderTransaction;
            return nhTransaction != null ? nhTransaction.NhSession : GetNHibernateSession(isReadonly);
        }

        public Configuration Config { get; protected set; }
        public ISessionFactory NhSessionFactory { get; protected set; }
        public ReaderWriterLockSlim Locker
        {
            get { return _locker; }
        }
        public ISession SingleProvidedSession { get; private set; }
        public bool LeaveSessionOpenOnDispose { get; private set; }
        public bool IsSingleSessionFinalized { get; set; }
        public IFrameworkContext FrameworkContext { get; private set; }
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        protected void EnsureFlushMode(ISessionImplementor session, bool isReadOnly)
        {
            if (session != null)
            {
                // If we're changing the FlushMode on an existing session from Auto,
                // then issue one flush now
                if (session.FlushMode == FlushMode.Auto && isReadOnly)
                    session.Flush();

                session.FlushMode = isReadOnly ? FlushMode.Never : FlushMode.Auto;
            }
        }


        internal static void UnbindAndCloseSession(ISessionFactory nhSessionFactory)
        {
            if (nhSessionFactory == null || nhSessionFactory.IsClosed) return;

            LogHelper.TraceIfEnabled<EntityRepositoryFactory>("Unbinding session and closing it");

            var session = CurrentSessionContext.Unbind(nhSessionFactory);
            if (session != null && session.IsOpen) session.Close();
        }

        internal ISessionFactory GetNHibernateSessionFactory()
        {
            //if we have config, then we may need to build the session factory
            if (Config != null && NhSessionFactory == null)
            {
                using (new WriteLockDisposable(Locker))
                {
                    NhSessionFactory = Config.BuildSessionFactory();
                }
            }

            if (NhSessionFactory == null && SingleProvidedSession != null)
                throw new NotSupportedException("When a single session is used to construct the EntityRepositoryFactory then no ISessionFactory is available");

            return NhSessionFactory;
        }

        /// <summary>
        /// Retursn the NHibernate session to use
        /// </summary>
        /// <returns></returns>
        internal ISession GetNHibernateSession(bool isReadonly, bool forceNewUnbounded = false)
        {
            //if a session has been supplied via the constructor (i.e. for testing)
            if (SingleProvidedSession != null)
            {
                if (!IsSingleSessionFinalized)
                {
                    using (new WriteLockDisposable(Locker))
                    {
                        IsSingleSessionFinalized = true;
                        //add the session to the finalizer
                        FrameworkContext.ScopedFinalizer.AddFinalizerToScope(SingleProvidedSession, x => UnbindAndCloseSession(NhSessionFactory));
                    }
                }


                EnsureFlushMode(SingleProvidedSession as ISessionImplementor, isReadonly);

                return SingleProvidedSession;
            }


            // See http://nhforge.org/doc/nh/en/index.html#architecture-current-session
            // for details on contextual sessions

            var sessionFactory = GetNHibernateSessionFactory();

            if (!forceNewUnbounded)
            {
                //if we have a session factory, then check if its bound
                if (sessionFactory != null && CurrentSessionContext.HasBind(sessionFactory))
                {
                    LogHelper.TraceIfEnabled<EntityRepositoryFactory>("GetNHibernateSession: using ISessionFactory.GetCurrentSession()");
                    var boundSession = NhSessionFactory.GetCurrentSession();

                    EnsureFlushMode(boundSession as ISessionImplementor, isReadonly);

                    return boundSession;
                }    
            }

            LogHelper.TraceIfEnabled<EntityRepositoryFactory>("GetNHibernateSession: using ISessionFactory.OpenSession()");

            var sessionToReturn = NhSessionFactory.OpenSession();

            EnsureFlushMode(sessionToReturn as ISessionImplementor, isReadonly);

            //add the new session to the finalizer
            FrameworkContext.ScopedFinalizer.AddFinalizerToScope(sessionToReturn, x => UnbindAndCloseSession(NhSessionFactory));

            if (!forceNewUnbounded)
            {
                //dont bind the session if we're forcing new for nested transactions
                CurrentSessionContext.Bind(sessionToReturn);    
            }

            
            return sessionToReturn;
        }

        protected override void DisposeResources()
        {
            if (NhSessionFactory == null) return;
            UnbindAndCloseSession(NhSessionFactory);
            NhSessionFactory.Dispose();
        }

        public NhProviderTransaction GenerateSessionAndTransaction(bool isReadOnly, out ISession session)
        {
            //get the session
            NhProviderTransaction transaction;
            session = GetNHibernateSession(isReadOnly);
            if (session.Transaction.IsActive)
            {
                //we have a nested transaction, lets create a new session
                //session = NhDependencyHelper.FactoryHelper.GetNHibernateSession(true);
                transaction = new NhProviderTransaction(session, true);
            }
            else
            {
                transaction = new NhProviderTransaction(session);
            }
            return transaction;
        }
    }
}