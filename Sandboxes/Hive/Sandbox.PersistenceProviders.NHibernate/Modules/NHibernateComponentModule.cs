using System;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Reflection;
using Autofac;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.ByteCode.Castle;
using NHibernate.Tool.hbm2ddl;
using Sandbox.PersistenceProviders.NHibernate.Mappings;
using Sandbox.PersistenceProviders.NHibernate.Mappings.Conventions;
using Configuration = NHibernate.Cfg.Configuration;
using Environment = NHibernate.Cfg.Environment;
using Module = Autofac.Module;

namespace Sandbox.PersistenceProviders.NHibernate.Modules
{
  public class NHibernateComponentModule : Module
  {
    public NHibernateComponentModule()
    {
      // TODO: should wrap the ConfigurationManager for unit testing
      ConnectionString =
        ConfigurationManager.ConnectionStrings["Sandbox.PersistenceProviders.NHibernate"].ConnectionString;
      AssemblyMapper = typeof (EntityMapping).Assembly;
    }

    public ISessionFactory SessionFactory { get; private set; }

    public string ConnectionString { get; set; }

    public Assembly AssemblyMapper { get; set; }

    protected override void Load(ContainerBuilder builder)
    {
      Contract.Assert(builder != null, "Builder container is null");
      Contract.Assert(!string.IsNullOrEmpty(ConnectionString), "Cannot find connection string in config file");
      Contract.Assert(AssemblyMapper != null, "AssemblyMapper is null");

      Configuration cfg = BuildConfiguration();

      AutoPersistenceModel persistenceModel = BuildPersistenceModel();
      persistenceModel.Configure(cfg);

      SessionFactory = BuildSessionFactory(cfg);

      RegisterConponents(builder, cfg);
    }

    public Configuration BuildConfiguration()
    {
        string directoryName = System.IO.Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().GetName().CodeBase);
        var directoryPath = new Uri(directoryName).LocalPath;
        var connectionString = ConnectionString.Replace("{bin}", directoryPath);

        //TODO: Need to have this inferred from configuration to support different dbs
      Configuration config = Fluently.Configure()
          .Database(MsSqlCeConfiguration.Standard.ConnectionString(connectionString))
        //.Database(MsSqlConfiguration.MsSql2005.ConnectionString(ConnectionString))
        .ExposeConfiguration(c => c.SetProperty(Environment.ReleaseConnections, "on_close"))
        .ExposeConfiguration(
          c => c.SetProperty(Environment.ProxyFactoryFactoryClass, typeof (ProxyFactoryFactory).AssemblyQualifiedName))
        .ExposeConfiguration(c => c.SetProperty(Environment.Hbm2ddlAuto, "create"))
        .ExposeConfiguration(c => c.SetProperty(Environment.ShowSql, "true"))
        .Mappings(x => x.FluentMappings.AddFromAssembly(AssemblyMapper))
        .ExposeConfiguration(BuildSchema)
        .BuildConfiguration();

      if (config == null)
        throw new Exception("Cannot build NHibernate configuration");

      return config;
    }

    public AutoPersistenceModel BuildPersistenceModel()
    {
      var persistenceModel = new AutoPersistenceModel();

      persistenceModel.Conventions.Setup(c =>
                                           {
                                             c.Add(typeof (ForeignKeyNameConvention));
                                             c.Add(typeof (ReferenceConvention));
                                             c.Add(typeof (PrimaryKeyNameConvention));
                                             c.Add(typeof (TableNameConvention));
                                           });

      persistenceModel.AddMappingsFromAssembly(AssemblyMapper);

      persistenceModel.BuildMappings();

      persistenceModel.WriteMappingsTo(@"./");

      return persistenceModel;
    }

    public ISessionFactory BuildSessionFactory(Configuration config)
    {
      ISessionFactory sessionFactory = config.BuildSessionFactory();

      if (sessionFactory == null)
        throw new Exception("Cannot build NHibernate Session Factory");

      return sessionFactory;
    }

    public void RegisterConponents(ContainerBuilder builder, Configuration config)
    {
      builder.RegisterInstance(config).As<Configuration>().SingleInstance();

      builder.RegisterInstance(SessionFactory).As<ISessionFactory>().SingleInstance();

      builder.Register(x => x.Resolve<ISessionFactory>().OpenSession()).As<ISession>().InstancePerLifetimeScope();
    }

    private static void BuildSchema(Configuration config)
    {
      new SchemaExport(config).SetOutputFile(@"./Schema.sql").Create(true, true);
    }
  }
}