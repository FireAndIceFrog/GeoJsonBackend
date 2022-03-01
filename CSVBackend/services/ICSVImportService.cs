using Newtonsoft.Json.Linq;

namespace CSVBackend.services
{
    public interface ICSVImportService
    {
        Task<JArray> GetWeeklyDataAsync();
        Task<bool> SetWeeklyDataAsync(string weeklyData);
    }
}