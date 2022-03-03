using System.Text.Json;

namespace CSVBackend.services
{
    public interface ICSVImportService
    {
        Task<bool> ClearAllData();
        Task<string> GetWeeklyDataAsync(int partNumber = 0);
        Task<bool> SetWeeklyDataAsync(JsonElement weeklyData);
        Task UpdateWeeklyDataAsync(JsonElement weeklyData, int part);
        Task<int> CountAllData();
    }
}