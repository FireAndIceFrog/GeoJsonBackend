using MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json;

namespace CSVBackend.services
{
    public class CSVImportService : ICSVImportService
    {
        private readonly IMongoDBConnector _mongoDataAccess;
        public CSVImportService(IMongoDBConnector mongoDataAccess)
        {
            _mongoDataAccess = mongoDataAccess;
        }

        public async Task<string> GetWeeklyDataAsync()
        {
            var documents = await _mongoDataAccess.GetDocumentsAsync("csv");
            var documentsList = documents.ToList();
            documentsList.Sort((x, y) => -((BsonTimestamp)x.GetValue("timestamp")).CompareTo((BsonTimestamp)y.GetValue("timestamp")));
            return documentsList[0]["data"].ToJson();
        }

        public async Task<bool> SetWeeklyDataAsync(JsonElement weeklyData)
        {
            var jsonData =  Newtonsoft.Json.JsonConvert.SerializeObject(new { data = Newtonsoft.Json.JsonConvert.DeserializeObject(weeklyData.ToString()) });
            var document = new BsonDocument();
            var success = BsonDocument.TryParse(jsonData, out document);
            document.InsertAt(0, new BsonElement("timestamp", new BsonTimestamp(0, 0)));
            await _mongoDataAccess.InsertOneAsync("csv", document);
            return true;
        }
    }
}
