using MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json.Linq;

namespace CSVBackend.services
{
    public class CSVImportService : ICSVImportService
    {
        private readonly IMongoDBConnector _mongoDataAccess;
        public CSVImportService(IMongoDBConnector mongoDataAccess)
        {
            _mongoDataAccess = mongoDataAccess;
        }

        public async Task<JArray> GetWeeklyDataAsync()
        {
            await Task.CompletedTask;
            return new JArray();
        }

        public async Task<bool> SetWeeklyDataAsync(string weeklyData)
        {
            var document = BsonSerializer.Deserialize<BsonDocument>(weeklyData);
            await _mongoDataAccess.InsertOneAsync("csv", document);
            return true;
        }
    }
}
