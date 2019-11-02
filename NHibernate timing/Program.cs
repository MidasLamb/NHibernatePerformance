using NHibernate_timing.NHibernateMapping;
using NHibernate_timing.SQLite;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Reflection;

namespace NHibernate_timing
{
    class Program
    {
        public static long amountOfIterations = 10000;
        static void Main(string[] args)
        {
            List<PerformanceTimer> pts = new List<PerformanceTimer>();
            // Pure NHibernate
            pts.Add(NHibernate());

            // Pure SQLite
            pts.Add(Raw());

            pts.Add(Mixed());
            var mwr = MixedWithRead();
            pts.Add(mwr.Item1);
            pts.Add(mwr.Item2);
            var mwsr = MixedWithSQLiteRead();
            pts.Add(mwsr.Item1);
            pts.Add(mwsr.Item2);

            foreach(var pt in pts)
            {
                pt.Print();
            }

            Console.ReadKey();
        }

        private static PerformanceTimer Raw()
        {
            SQLiteSetup setup = new SQLiteSetup("SQLite.db3");
            setup.CreateTable();
            var con = setup.GetConnection();


            PerformanceTimer pt = new PerformanceTimer("Raw SQLite");

            con.Open();
            using (var trans = con.BeginTransaction())
            {
                string sql = "INSERT INTO User (Name, LastName) VALUES (@name, @lastName);";
                SQLiteCommand command = new SQLiteCommand(sql,  con);
                command.Parameters.Add("@name", System.Data.DbType.String);
                command.Parameters.Add("@lastName", System.Data.DbType.String);
                command.Transaction = trans;

                for (int i = 0; i < amountOfIterations; i++)
                {
                    pt.NewIteration();
                    pt.Start("Parameter and insert");

                    command.Parameters["@name"].Value = $"Midas{i}";
                    command.Parameters["@lastName"].Value = $"Lam{i}";

                    command.ExecuteNonQuery();
                    pt.Stop();
                }
            }

            return pt;

        }

        private static PerformanceTimer NHibernate()
        {
            var setup = new NHibernateSetup("Nhibernate.db3");
            var session = setup.OpenSession();


            PerformanceTimer pt = new PerformanceTimer("Nhibernate");

            using (var trans = session.BeginTransaction())
            {
                for (int i = 0; i < amountOfIterations; i++)
                {
                    pt.NewIteration();
                    pt.Start("NHibernate Insert");
                    session.Insert(new User
                    {
                        Name = $"Midas {i}",
                        LastName = "Lambrichts"
                    });
                    pt.Stop();
                }

                trans.Commit();
            }

            return pt;
        }

        private static PerformanceTimer Mixed()
        {
            var setup = new NHibernateSetup("NhibernateMixed.db3");
            var session = setup.OpenSession();


            PerformanceTimer pt = new PerformanceTimer("mixed");

            using (var nTrans = session.BeginTransaction())
            {
                SQLiteTransaction trans = (SQLiteTransaction) nTrans.GetType().GetField("trans", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(nTrans);

                string sql = "INSERT INTO User (Name, LastName) VALUES (@name, @lastName);";
                SQLiteCommand command = new SQLiteCommand(sql, trans.Connection);
                command.Parameters.Add("@name", System.Data.DbType.String);
                command.Parameters.Add("@lastName", System.Data.DbType.String);
                command.Transaction = trans;

                for (int i = 0; i < amountOfIterations; i++)
                {
                    pt.NewIteration();
                    pt.Start("SQLite Insert inside NHibernate session");

                    command.Parameters["@name"].Value = $"Midas{i}";
                    command.Parameters["@lastName"].Value = $"Lam{i}";

                    command.ExecuteNonQuery();
                    pt.Stop();
                }
            }

            return pt;
        }

        private static (PerformanceTimer, PerformanceTimer) MixedWithRead()
        {
            var setup = new NHibernateSetup("NhibernateMixedRead.db3");
            var session = setup.OpenSession();


            PerformanceTimer pt = new PerformanceTimer("Mixed with read");
            PerformanceTimer ptOuter = new PerformanceTimer("Mixed with read entire");

            using (var nTrans = session.BeginTransaction())
            {
                SQLiteTransaction trans = (SQLiteTransaction) nTrans.GetType().GetField("trans", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(nTrans);

                string sql = "INSERT INTO User (Name, LastName) VALUES (@name, @lastName);";
                SQLiteCommand command = new SQLiteCommand(sql, trans.Connection);
                command.Parameters.Add("@name", System.Data.DbType.String);
                command.Parameters.Add("@lastName", System.Data.DbType.String);
                command.Transaction = trans;

                for (int i = 0; i < amountOfIterations; i++)
                {
                    pt.NewIteration();
                    ptOuter.NewIteration();

                    ptOuter.Start("NHibernate read and SQLite insert");
                    session.QueryOver<User>().Where(x => x.Id == i - 1).List();
                    pt.Start("SQLite Insert inside NHibernate after NHibernate read");

                    command.Parameters["@name"].Value = $"Midas{i}";
                    command.Parameters["@lastName"].Value = $"Lam{i}";

                    command.ExecuteNonQuery();
                    pt.Stop();
                    ptOuter.Stop();
                }
            }

            return (ptOuter, pt);
        }

        private static (PerformanceTimer, PerformanceTimer) MixedWithSQLiteRead()
        {
            var setup = new NHibernateSetup("NhibernateMixedSQLiteRead.db3");
            var session = setup.OpenSession();


            PerformanceTimer pt = new PerformanceTimer("Mixed with SQLite read");
            PerformanceTimer ptOuter = new PerformanceTimer("Mixed with SQLite read outer");

            using (var nTrans = session.BeginTransaction())
            {
                SQLiteTransaction trans = (SQLiteTransaction) nTrans.GetType().GetField("trans", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(nTrans);

                string sql = "INSERT INTO User (Name, LastName) VALUES (@name, @lastName);";
                SQLiteCommand command = new SQLiteCommand(sql, trans.Connection);
                command.Parameters.Add("@name", System.Data.DbType.String);
                command.Parameters.Add("@lastName", System.Data.DbType.String);
                command.Transaction = trans;

                string readSql = "SELECT * FROM User WHERE Id = @id";
                SQLiteCommand readCommand = new SQLiteCommand(readSql, trans.Connection);
                readCommand.Transaction = trans;
                readCommand.Parameters.Add("@id", System.Data.DbType.Int32);

                for (int i = 0; i < amountOfIterations; i++)
                {
                    pt.NewIteration();
                    ptOuter.NewIteration();

                    ptOuter.Start("SQLite Read and Insert inside NHibernate transaction");
                    readCommand.Parameters["@id"].Value = i - 1;

                    var reader = readCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        var x = reader.GetInt32(0);
                    }

                    reader.Close();

                    pt.Start("SQLite Insert inside NHibernate after SQLite read");

                    command.Parameters["@name"].Value = $"Midas{i}";
                    command.Parameters["@lastName"].Value = $"Lam{i}";

                    command.ExecuteNonQuery();
                    pt.Stop();
                    ptOuter.Stop();
                }
            }

            return (ptOuter, pt);
        }
    }
}
