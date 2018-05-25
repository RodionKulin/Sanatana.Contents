using MongoDB.Bson;
using MongoDB.Driver;

namespace Sanatana.Contents.Database.MongoDb.Context
{
    public interface IContentMongoDbContext
    {
        IMongoCollection<TDocument> GetCollection<TDocument>();
        IMongoDatabase Database { get; }
    }
}