using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NHibernate_timing.NHibernateMapping
{
    public class NHibernateSetup
    {

        private ISessionFactory SessionFactory { get; set; }
        
        public NHibernateSetup(string dbName)
        {
            this.SessionFactory = CreateSessionFactory(dbName);
        }

        private ISessionFactory CreateSessionFactory(string dbName)
        {
            var connectionString = $"Data Source={dbName};Version=3;New=True;Compress=True;";

            Configuration config = null;
            var sessionFactory = Fluently.Configure()
                .Database(
                    //SQLiteConfiguration.Standard
                    //.UsingFile(dbName)
                    SQLiteConfiguration.Standard.ConnectionString(connectionString)
                )
                .Mappings(m =>
                            m.FluentMappings.AddFromAssemblyOf<Program>())
                .ExposeConfiguration(c =>
                {
                    config = c; //pass configuration to class scoped variable
                })

                .BuildSessionFactory();

            var session = sessionFactory.OpenSession();

            new SchemaExport(config).Execute(true, true, false, session.Connection, null);

            session.Close();


            return sessionFactory;
        }

        public IStatelessSession OpenSession()
        {
            return SessionFactory.OpenStatelessSession();
        }
    }
}
