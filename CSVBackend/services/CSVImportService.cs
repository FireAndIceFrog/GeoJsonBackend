using MongoDB;
using MongoDB.Bson;
using System.Globalization;
using System.Text.Json;

namespace CSVBackend.services
{
    public class CSVImportService : ICSVImportService
    {
        private readonly IMongoDBConnector _mongoDataAccess;
        public CSVImportService(IMongoDBConnector mongoDataAccess)
        {
            _mongoDataAccess = mongoDataAccess;

        }
        private string _collectionName { get => $"csv_{_currentWeek}"; }
        private int _currentWeek { get => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday); }
        private async Task<BsonDocument?> GetDocuments(int partNumber = 0)
        {
            var documents = await _mongoDataAccess.GetDocumentsAsync(_collectionName);
            if (!documents.Any())
            {
                return null;
            }

            var documentsList = documents.ToList();
            documentsList.Sort((x, y) => ((BsonTimestamp)x.GetValue("timestamp")).CompareTo((BsonTimestamp)y.GetValue("timestamp")));

            if (documentsList[partNumber] != null)
            {
                return documentsList[partNumber];
            }

            return null;
        }

        private BsonDocument? ConvertJsonToBson(JsonElement weeklyData)
        {
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject(weeklyData.ToString());
            var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                data =
                new List<object>(1)
                {
                    data ?? new { }
                }
            });
            var success = BsonDocument.TryParse(jsonData, out var document);
            document?.InsertAt(0, new BsonElement("timestamp", new BsonTimestamp(0, 0)));
            return document;
        }

        public async Task<string> GetWeeklyDataAsync(int partNumber = 0)
        {
            var document = await GetDocuments(partNumber);

            if (document != null && document["data"] != null)
            {
                document.TryGetValue("data", out var value);
                if (value != null)
                {
                    //first index of data
                    return value[0].ToString() ?? "";
                }
            }

            return "";
        }

        public async Task<bool> SetWeeklyDataAsync(JsonElement weeklyData)
        {
            var document = ConvertJsonToBson(weeklyData);
            if (document != null)
            {
                await _mongoDataAccess.InsertOneAsync(_collectionName, document);
                return true;
            }
            return false;
        }

        public async Task<int> CountAllData()
        {
            var documents = await _mongoDataAccess.GetDocumentsAsync(_collectionName);
            var dataList = documents.Select(document => document["data"][0]);
            var counts = dataList.AsParallel().Select(document => document["Rows"].AsBsonArray.Count);
            return counts.Aggregate(0, (total, count) => total + count);
        }

        public async Task UpdateWeeklyDataAsync(JsonElement weeklyData, int part)
        {
            var oldDoc = await GetDocuments(part);
            var updateDoc = ConvertJsonToBson(weeklyData);
            if (oldDoc != null && updateDoc != null)
            {
                await _mongoDataAccess.UpdateOneAsync(_collectionName, oldDoc, updateDoc);
            }
        }

        public async Task<bool> ClearAllData()
        {
            var docs = await _mongoDataAccess.GetDocumentsAsync(_collectionName);
            var tasks = docs.AsParallel().Select(async (x) => await _mongoDataAccess.DeleteDocAsync(_collectionName, x));
            await Task.WhenAll(tasks);
            return true;
        }
    }
}
