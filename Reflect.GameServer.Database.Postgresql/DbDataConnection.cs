using LinqToDB;
using LinqToDB.Data;
using Reflect.GameServer.Database.Postgresql.Models;

namespace Reflect.GameServer.Database.Postgresql
{
    public class DbDataConnection : DataConnection
    {
        public DbDataConnection() : base("PostgreSQL")
        {
        }

        public ITable<User> User => GetTable<User>();
    }
}