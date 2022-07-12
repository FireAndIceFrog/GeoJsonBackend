using MongoDB.Bson;
using MongoDB.Driver;

namespace CSVBackend.Map.DataAccess
{
    public class GeoJsonDataAccess : IGeoJsonDataAccess
    {
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
