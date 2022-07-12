using MongoDB.Bson;
using MongoDB.Driver;

namespace CSVBackend.Map.DataAccess
{
    public interface IGeoJsonDataAccess
    {
        BsonDocument[] GetFeaturePipelineByPoint(double lat, double lon);
    }
}