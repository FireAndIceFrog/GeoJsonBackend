using System.Text.Json;

namespace CSVBackend.services
{
    public interface ICSVImportService
    {
        Task<bool> ClearAllData();
        Task<Guid> GetMostRecentTableId();
        Task<string> GetRows(Guid? id, int startRow, int endRow);
        Task<string> GetTableHeadersStringAsync(Guid? id);
        Task SaveTableDataAsync(Guid id, JsonElement tableData);
        Task<string> SaveTableHeadersAsync(JsonElement headers, bool createNewId = true);
    }
}