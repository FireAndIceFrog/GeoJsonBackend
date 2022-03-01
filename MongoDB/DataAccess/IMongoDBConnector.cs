using MongoDB.Bson;

namespace MongoDB
{
    public interface IMongoDBConnector
    {
        Task InsertOneAsync(string collectionName, BsonDocument document);
    }
}