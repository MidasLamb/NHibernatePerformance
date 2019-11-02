using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace NHibernate_timing.SQLite
{
    public class SQLiteSetup
    {

        private SQLiteConnection connection;
        public SQLiteSetup(string dbName)
        {
            connection = new SQLiteConnection($"Data Source={dbName};Version=3;New=True;Compress=True;");
        }

        public SQLiteConnection GetConnection()
        {
            return connection;
        }

        public void CreateTable()
        {
            string sql = "CREATE TABLE \"User\" (Id  integer primary key autoincrement, Name TEXT, LastName TEXT)";

            connection.Open();
            var command = new SQLiteCommand(sql, connection);

            try
            {

                command.ExecuteNonQuery();
            } catch (Exception)
            {

            }
            connection.Close();
        }
    }
}
