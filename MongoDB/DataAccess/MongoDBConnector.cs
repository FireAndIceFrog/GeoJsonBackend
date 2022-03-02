using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoDB;

public class MongoDBConnector : IMongoDBConnector
{
    private readonly MongoClient _client;
    private readonly string _dbName;

    public MongoDBConnector(string connString, string dbName)
    {
        _client = new MongoClient(connString);
        _dbName = dbName;
    }

    private IMongoDatabase Database { get => _client.GetDatabase(_dbName); }
    private IMongoCollection<BsonDocument> getCollection(string collectionName) => Database.GetCollection<BsonDocument>(collectionName);

    public async Task InsertOneAsync(string collectionName, BsonDocument document)
    {
        await getCollection(collectionName).InsertOneAsync(document);
    }

    public async Task<IMongoQueryable<BsonDocument>> GetDocumentsAsync(string collectionName)
    {
        var data = await Task.FromResult(getCollection(collectionName).AsQueryable());
        return data;
    }

}
