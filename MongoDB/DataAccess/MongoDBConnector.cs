using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
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

    public async Task InsertManyAsync(string collectionName, IEnumerable<BsonDocument> documents)
    {
        await getCollection(collectionName).InsertManyAsync(documents);
    }

    public async Task<UpdateResult> UpdateOneAsync(string collectionName, FilterDefinition<BsonDocument> oldDoc, BsonDocument document)
    {
        return await getCollection(collectionName).UpdateOneAsync(oldDoc, document);
    }

    public async Task<ReplaceOneResult> ReplaceOneAsync(string collectionName, FilterDefinition<BsonDocument> oldDoc, BsonDocument document)
    {
        return await getCollection(collectionName).ReplaceOneAsync(oldDoc, document);
    }

    public async Task<IMongoQueryable<BsonDocument>> GetDocumentsAsync(string collectionName)
    {
        var data = await Task.FromResult(getCollection(collectionName).AsQueryable());
        return data;
    }

    public async Task DeleteDocAsync(string collectionName, FilterDefinition<BsonDocument> document)
    {
        await getCollection(collectionName).DeleteOneAsync(document);
    }

    public async Task DeleteManyAsync(string collectionName, FilterDefinition<BsonDocument> document)
    {
        await getCollection(collectionName).DeleteManyAsync(new BsonDocument());
    }

    public async Task<IAsyncCursor<BsonDocument>> FindNearAsync(string collectionName, double x, double y)
    {
        var point = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(x, y));
        var builder = Builders<BsonDocument>.Filter;
        var filter = builder.Near("geometry", point);
        var docs = getCollection(collectionName);

        return await docs.FindAsync(filter);
    }
}
