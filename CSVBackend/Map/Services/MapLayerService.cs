using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace CSVBackend.Map.Services;

public class MapLayerService : IMapLayerService
{
    private readonly IMongoDBConnector _mongoDataAccess;
    public MapLayerService(IMongoDBConnector mongoDataAccess)
    {
        _mongoDataAccess = mongoDataAccess;

    }
    private string _collectionName { get => $"map_layers"; }
    private string _tableHeaderName { get => $"{_collectionName}_headers"; }
    private int _currentWeek { get => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday); }
    private readonly string timestampField = "timestamp";
    private readonly string headerIdField = "HeaderId";
    private Guid _id = Guid.Empty;

    public async Task<string> GetFeatures(double x, double y, double z)
    {
        var BsonId = new BsonBinaryData(_id, GuidRepresentation.Standard);
        var asyncDocs = await _mongoDataAccess.FindNearAsync(_collectionName, x, y);

        var docs = await asyncDocs.ToListAsync();
        var processedDocs = docs
            .Select((doc) =>
            {
                var id = doc.GetValue("_id").AsObjectId;
                doc.Remove("_id");

                var properties = doc.GetValue("properties");
                properties.AsBsonDocument.Set("id", id.ToString());
                doc.Set("properties", properties);
                return doc;
            });



        return new  { features = processedDocs }.ToJson();
    }

    public async Task CreateFeature(object data)
    {
        var actualVal = BsonDocument.Parse(data.ToString());
        if (actualVal != null)
        {
            await _mongoDataAccess.InsertOneAsync(_collectionName, actualVal);
        }
    }

    public async Task<bool> ClearAllData()
    {

        await _mongoDataAccess.DeleteAllAsync(_collectionName);
        await _mongoDataAccess.DeleteAllAsync(_tableHeaderName);

        return true;
    }
}
