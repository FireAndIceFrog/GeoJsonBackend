using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CSVBackend.Map.DataAccess
{
    public class GeoJsonDataAccess : IGeoJsonDataAccess
    {
        public const string PropertiesHistoryCollection = "feature_properties_history";

        private readonly IMongoDBConnector _mongoDataAccess;
        public GeoJsonDataAccess(IMongoDBConnector mongoDataAccess)
        {
            _mongoDataAccess = mongoDataAccess;

        }

        public async Task DeleteFeaturePropertyHistory(List<string> data)
        {
            var builder = Builders<BsonDocument>.Filter;
            var containsFeatureIdList = data!.Select((id) => builder.Eq("feature_id", new ObjectId(id)));
            var containsFeatureFilter = builder.Or(
                containsFeatureIdList
            );

            await _mongoDataAccess.DeleteManyAsync(PropertiesHistoryCollection, containsFeatureFilter);
        }

        public async Task SaveFeaturePropertyChangesToPipeline(string collectionName) 
        {
            var pipeline =  new []
            {
                new BsonDocument("$set",
                    new BsonDocument
                        {
                            { "properties.feature_id", "$_id" },
                            { "properties.version", "$version" }
                        }),
                    new BsonDocument("$replaceRoot",
                    new BsonDocument("newRoot", "$properties")),
                    new BsonDocument("$merge",
                        new BsonDocument
                        {
                            { "into", PropertiesHistoryCollection },
                            { "on",
                                new BsonArray
                                {
                                    "feature_id",
                                    "version"
                                } 
                            },
                            { "whenMatched", "keepExisting" },
                            { "whenNotMatched", "insert" }
                        })
            };

            await _mongoDataAccess.RunPipelineAsync<BsonDocument>(collectionName, pipeline);
        }

        public BsonDocument[] GetFeaturePipelineByPoint(double lat, double lon)
        {
            var pipeline = new[]
            {
                new BsonDocument("$geoNear", new BsonDocument
                {
                    { "near", new BsonDocument
                        {
                            { "type", "Point" },
                            { "coordinates", new BsonArray
                                {
                                    lat,
                                    lon
                                } 
                            }
                        }
                    },
                    { "distanceField", "distance" }
                }),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "feature_properties" },
                    { "pipeline", new BsonArray() },
                    { "localField", "_id" },
                    { "foreignField", "feature_id" },
                    { "as", "properties" }
                }),
                new BsonDocument("$set",
                    new BsonDocument("properties",
                        new BsonDocument("$arrayToObject",
                            new BsonDocument("$concatArrays",
                                new BsonArray
                                {
                                    new BsonDocument("$map", new BsonDocument
                                    {
                                        { "input", "$properties" },
                                        { "as", "item" },
                                        { "in", new BsonDocument
                                            {
                                                { "k", "$$item.key" },
                                                { "v", "$$item.value" }
                                            } 
                                        }
                                    }),
                                    new BsonArray
                                    {
                                        new BsonDocument
                                        {
                                            { "k", "version" },
                                            { "v", new BsonDocument("$max", "$properties.version") }
                                        }
                                    }
                                }
                            )
                        )
                    )
                )
            };
            return pipeline;
        }


    }
}
