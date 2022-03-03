using MongoDB.Bson;
using MongoDB.Driver.Linq;

namespace MongoDB
{
    public interface IMongoDBConnector
    {
        Task InsertOneAsync(string collectionName, BsonDocument document);
        Task<IMongoQueryable<BsonDocument>> GetDocumentsAsync(string collectionName);
        Task DeleteDocAsync(string collectionName, BsonDocument document);
        Task UpdateOneAsync(string collectionName, BsonDocument oldDoc, BsonDocument document);
    }
}