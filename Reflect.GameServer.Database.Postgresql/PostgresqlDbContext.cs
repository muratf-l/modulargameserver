using System.Linq;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Configuration;
using Reflect.GameServer.Database.Postgresql.Context;
using Reflect.GameServer.Database.Postgresql.Models;

namespace Reflect.GameServer.Database.Postgresql
{
    public class PostgresqlDbContext : IPostgresqlDbContext
    {
        public PostgresqlDbContext(IConfiguration diConfig)
        {
            var section = diConfig.GetSection("Postgresql");

            DataConnection.DefaultSettings = new NSettings
            {
                Settings = new ConnectionStringSettings
                {
                    ConnectionString = section["ConnectionString"],
                    Name = "PostgreSQL",
                    ProviderName = "PostgreSQL"
                }
            };
#if DEBUG
            //using var db = new DbDataConnection();

            //var sp = db.DataProvider.GetSchemaProvider();

            //var dbSchema = sp.GetSchema(db);

            //if (dbSchema.Tables.All(t => t.TableName != db.User.TableName))
            //{
            //    db.CreateTable<User>();
            //}
#endif
        }
    }
}