using CSVBackend.Map.DataAccess;
using CSVBackend.Map.models;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace CSVBackend.Map.Services
{
    public class MapLayerServiceV2 : IMapLayerService
    {
        private readonly IGeoJsonDataAccess _geoJsonDataAccess;
        private readonly IMongoDBConnector _mongoDataAccess;
        private const string _featuresCollection = "features";

        public MapLayerServiceV2(IGeoJsonDataAccess geoJsonDataAccess, IMongoDBConnector mongoDataAccess)
        {
            _geoJsonDataAccess = geoJsonDataAccess;
            _mongoDataAccess = mongoDataAccess;
        }
        private BsonDocument CreateDocument<T>(GeoJsonModel<T> model)
        {
            return new BsonDocument()
            {
                new BsonElement("type", new BsonString(model?.type)),
                new BsonElement("geometry", new BsonDocument()
                {
                    new BsonElement("type", new BsonString(model?.geometry?.type)),
                    new BsonElement("coordinates", new BsonArray(model?.geometry?.coordinates)),
                }),
                new BsonElement("properties", new BsonDocument()
                {
                    new BsonElement("backgroundColor", new BsonString(model?.properties?.backgroundColor)),
                    new BsonElement("borderColor", new BsonString(model?.properties?.borderColor)),
                })
            };
        }

        public async Task<string> GetFeatures(double x, double y, double z)
        {
            var pipeline = _geoJsonDataAccess.GetFeaturePipelineByPoint(x, y);
            var asyncDocs = await _mongoDataAccess.RunPipelineAsync<BsonDocument>(_featuresCollection, pipeline);

            var docs = await asyncDocs.ToListAsync();
            var processedDocs = docs
                .Select((doc) =>
                {
                    var id = doc.GetValue("_id").AsObjectId;
                    doc.Remove("_id");
                    doc.Remove("distance");

                    var properties = doc.GetValue("properties");
                    properties.AsBsonDocument.Set("id", id.ToString());
                    doc.Set("properties", properties);
                    return doc;
                });

            return new { features = processedDocs }.ToJson();
        }

        public Task<bool> ClearAllData()
        {
            throw new NotImplementedException();
        }

        public Task CreateFeature(object data)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFeaturesAsync(object data)
        {
            throw new NotImplementedException();
        }

        

        public Task UpdateFeatures(object data)
        {
            throw new NotImplementedException();
        }
    }
}
