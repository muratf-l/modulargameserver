using System;
using MongoDB.Driver;

namespace Reflect.GameServer.Database.Mongo
{
    public interface IMongoDbContext
    {
        IMongoDatabase Db { get; }

        string GetCollectionName(Type documentType);

        IMongoCollection<TDocument> GetCollection<TDocument>();
    }
}