using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Reflect.GameServer.Database.Mongo.Helpers;

namespace Reflect.GameServer.Database.Mongo
{
    public class MongoDbContext : IMongoDbContext
    {
        public MongoDbContext(IConfiguration diConfig)
        {
            var section = diConfig.GetSection("Mongo");

            var mongoClient = new MongoClient(section["ConnectionString"]);

            Db = mongoClient.GetDatabase(section["Database"]);
        }

        public IMongoDatabase Db { get; }

        public string GetCollectionName(Type documentType)
        {
            return ((BsonCollectionAttribute) documentType.GetCustomAttributes(
                    typeof(BsonCollectionAttribute),
                    true)
                .FirstOrDefault())?.CollectionName;
        }

        IMongoCollection<TDocument> IMongoDbContext.GetCollection<TDocument>()
        {
            return Db.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
        }
    }
}