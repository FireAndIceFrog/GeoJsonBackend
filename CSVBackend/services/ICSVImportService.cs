using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace CSVBackend.services
{
    public interface ICSVImportService
    {
        Task<string> GetWeeklyDataAsync();
        Task<bool> SetWeeklyDataAsync(JsonElement weeklyData);
    }
}