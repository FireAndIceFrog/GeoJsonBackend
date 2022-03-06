using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
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
        private string _tableHeaderName { get => $"{_collectionName}_headers"; }
        private int _currentWeek { get => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday); }
        private readonly string timestampField = "timestamp";
        private readonly string headerIdField = "HeaderId";
        private Guid _id = Guid.Empty;

        private BsonDocument? ConvertJsonToBson(JsonElement weeklyData)
        {
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject(weeklyData.ToString());
            var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            var success = BsonDocument.TryParse(jsonData, out var document);
            if(!success)
            {
                throw new Exception("Failed to parse document sent. Verify its an object not an array");
            }
            
            return document;
        }

        private IEnumerable<BsonDocument?> ConvertJsonArrayToBsonArray(JsonElement weeklyData)
        {
            IEnumerable<object>? data = (IEnumerable<object>?)Newtonsoft.Json.JsonConvert.DeserializeObject(weeklyData.ToString());
            if(data != null)
            {
                var dataArray = data.Select(x => {
                    return BsonDocument.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(x));
                });
                return (IEnumerable<BsonDocument?>)dataArray;
            }

            return new List<BsonDocument>();
        }

        private async Task<IMongoQueryable<BsonDocument>?> GetDocumentsAsync(string collection)
        {
            var documents = await _mongoDataAccess.GetDocumentsAsync(collection);
            if (!documents.Any())
            {
                return null;
            }

            if (documents != null)
            {
                return documents;
            }

            return null;
        }

        private async Task<BsonDocument?> GetTableHeadersAsync(Guid? id)
        {
            var docs = await GetDocumentsAsync(_tableHeaderName);
            if (docs == null || !docs.Any())
            {
                return null;
            }

            IMongoQueryable<BsonDocument> headers = docs.OrderBy(x => x[timestampField]);

            if (id != null && id != Guid.Empty)
            {
                var idData = new BsonBinaryData((Guid)id, GuidRepresentation.Standard);
                headers = headers.Where(x => x["_id"] == idData);
            }

            if (headers.Any())
            {
                return await headers.FirstAsync();
            }

            return null;
        }

        public async Task<string> SaveTableHeadersAsync(JsonElement headers, bool createNewId = true)
        {
            if (!createNewId || _id == Guid.Empty)
            {
                var tempId = await GetMostRecentTableId();

                if (tempId != Guid.Empty)
                {
                    _id = tempId;
                }
                else
                {
                    _id = Guid.NewGuid();
                }
            }
            else
            {
                _id = Guid.NewGuid();
            }

            var data = ConvertJsonToBson(headers);
            if (data != null)
            {
                data.InsertAt(0, new BsonElement(timestampField, new BsonTimestamp(0, 0)));
                data.InsertAt(0, new BsonElement("_id", new BsonBinaryData(_id, GuidRepresentation.Standard)));
                await _mongoDataAccess.InsertOneAsync(_tableHeaderName, data);

                return _id.ToString();
            }

            return "";
        }

        public async Task<Guid> GetMostRecentTableId()
        {
            var doc = await GetTableHeadersAsync(null);
            if (doc != null)
            {
                var success = doc.TryGetElement("_id", out var elm);
                if (success == true)
                {
                    BsonBinaryData idBson = elm.Value.AsBsonBinaryData;
                    return idBson.ToGuid();
                }
            }

            return Guid.Empty;
        }

        public async Task<string> GetTableHeadersStringAsync(Guid? id)
        {
            if (id == null || _id == Guid.Empty || id == Guid.Empty)
            {
                var tempId = await GetMostRecentTableId();
                _id = tempId;
            }

            var doc = await GetTableHeadersAsync(_id);
            if (doc != null)
            {
                var jsonDoc = Newtonsoft.Json.JsonConvert.DeserializeObject(doc["data"].AsBsonValue.ToString());
                return Newtonsoft.Json.JsonConvert.SerializeObject(new { data = jsonDoc });
            }
            return string.Empty;
        }

        public async Task SaveTableDataAsync(Guid id, JsonElement tableData)
        {
            var RowData = ConvertJsonArrayToBsonArray(tableData);
            if (RowData != null)
            {
                var rows = RowData.Select(x =>
                {
                    x[headerIdField] = new BsonBinaryData(_id, GuidRepresentation.Standard);
                    
                    return x;
                });

                if (rows != null)
                {
                    await _mongoDataAccess.InsertManyAsync(_collectionName, rows);
                }
                else
                {
                    throw new Exception("Rows cannot be null");
                }
                
            }
        }

        public async Task<string> GetRows(Guid? id, int startRow, int endRow)
        {
            if (id == null || _id == Guid.Empty)
            {
                var tempId = await GetMostRecentTableId();
                _id = tempId;
            }

            if (_id == Guid.Empty)
            {
                return "";
            }

            var BsonId = new BsonBinaryData(_id, GuidRepresentation.Standard);
            var docs = await _mongoDataAccess.GetDocumentsAsync(_collectionName);
            var counts = await docs.CountAsync();

            var filteredDocs = docs.Where(x => x[headerIdField] == BsonId).Skip(startRow).Take(endRow);


            var jsonParsedDocs = filteredDocs.ToList().Select(x => {
                
                return BsonTypeMapper.MapToDotNetValue(x);
            });


            return Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                counts,
                data = jsonParsedDocs
            });
        }

        public async Task<bool> ClearAllData()
        {

            await _mongoDataAccess.DeleteAllAsync(_collectionName);
            await _mongoDataAccess.DeleteAllAsync(_tableHeaderName);

            return true;
        }
    }
}
