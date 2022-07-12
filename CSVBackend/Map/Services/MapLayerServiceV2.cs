using CSVBackend.Map.DataAccess;
using CSVBackend.Map.models;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CSVBackend.Map.Services
{
    public class MapLayerServiceV2 : IMapLayerService
    {
        private readonly IGeoJsonDataAccess _geoJsonDataAccess;
        private readonly IMongoDBConnector _mongoDataAccess;
        private const string _featuresCollection = "features";
        private const string _propertiesCollection = "feature_properties";

        public MapLayerServiceV2(IGeoJsonDataAccess geoJsonDataAccess, IMongoDBConnector mongoDataAccess)
        {
            _geoJsonDataAccess = geoJsonDataAccess;
            _mongoDataAccess = mongoDataAccess;
        }
        private (BsonDocument, BsonDocument[]) CreateDocument<T>(GeoJsonModel<T> model)
        {
            var id = model?.properties?.id ==null ? 
                new ObjectId(model!.properties!.id) : ObjectId.GenerateNewId();

            var feature = new BsonDocument()
            {
                new BsonElement("_id", new BsonObjectId(id)),
                new BsonElement("type", new BsonString(model?.type)),
                new BsonElement("version", new BsonInt32((model?.version ?? -1) + 1 )),
                new BsonElement("geometry", new BsonDocument()
                {
                    new BsonElement("type", new BsonString(model?.geometry?.type)),
                    new BsonElement("coordinates", new BsonArray(model?.geometry?.coordinates)),
                }),
            };

            var properties = new BsonDocument[]
            {
                new BsonDocument()
                {
                    new BsonElement("feature_id", new BsonObjectId(id)),
                    new BsonElement("key", new BsonString("backgroundColor")),
                    new BsonElement("value", new BsonString(model!.properties.backgroundColor)),
                    new BsonElement("version", new BsonInt32((model.properties.version ?? -1) + 1 )),
                },

                new BsonDocument()
                {
                    new BsonElement("feature_id", new BsonObjectId(id)),
                    new BsonElement("key", new BsonString("borderColor")),
                    new BsonElement("value", new BsonString(model!.properties.borderColor)),
                    new BsonElement("version", new BsonInt32((model.properties.version ?? -1) + 1 )),
                },
            };

            return (feature, properties);
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

        public async Task CreateFeature(object data)
        {
            if (data == null)
            {
                return;
            }
            var deserializedData = JsonConvert.DeserializeObject<GeoJsonModelList<dynamic>>(data!.ToString()!);

            var tasks = deserializedData?.features?.Select((item) =>
            {
                item!.properties.id = null;
                if (item?.geometry?.coordinates != null)
                {
                    item.geometry.coordinates = item.geometry.coordinates.Select((ring) =>
                    {
                        return ring.Select((point) =>
                        {
                            if (point is JArray)
                                return (point as JArray)?.Select((coordinate) => double.Parse(coordinate.ToString()));
                            else
                                return double.Parse(point);
                        })
                        .ToList();
                    })
                    .ToList();
                }

                var (features, properties) = CreateDocument(item!);

                return Task.WhenAll(
                    _mongoDataAccess.InsertOneAsync(_featuresCollection, features),
                    _mongoDataAccess.InsertManyAsync(_propertiesCollection, properties)
                );

            })
            .ToArray();

            if (tasks != null && tasks.Any())
            {
                await Task.WhenAll(tasks);
            }
            return;
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
