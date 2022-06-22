using CSVBackend.Ryan.services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Models;
using System.Text.Json;

namespace CSVBackend.Ryan.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("MyPolicy")]
    public class MapLayerController : ControllerBase
    {

        private readonly ILogger<MapLayerController> _logger;
        private readonly ICSVImportService _csvImportService;

        public MapLayerController(ILogger<MapLayerController> logger, ICSVImportService csvImporter)
        {
            _logger = logger;
            _csvImportService = csvImporter;
        }

        [HttpPost]
        [Route("GetRows")]
        public async Task<string> GetRows([FromBody] object data, string? TableId, int start = 0, int numRows = 100)
        {
            var response = await _csvImportService.GetRows(TableId == null ? null : Guid.Parse(TableId), start, numRows, data);
            return response;
        }

        [HttpPost]
        [Route("SetRows")]
        public async Task SetWeeklyDataAsync([FromBody] object data, string TableId)
        {
            await _csvImportService.SaveTableDataAsync(Guid.Parse(TableId), (JsonElement)data);
        }

        [HttpGet]
        [Route("GetHeaders")]
        public async Task<string> GetHeaders(string? TableId)
        {
            var response = await _csvImportService.GetTableHeadersStringAsync(TableId == null ? null : Guid.Parse(TableId));
            return response;
        }

        [HttpGet]
        [Route("GetMostRecentTableId")]
        public async Task<string> GetMostRecentTableId()
        {
            var response = await _csvImportService.GetMostRecentTableId();
            return response.ToString();
        }

        [HttpPost]
        [Route("SetHeaders")]
        public async Task<string> SetHeaders([FromBody] object data, bool createNewId)
        {
            var tableId = await _csvImportService.SaveTableHeadersAsync((JsonElement)data, createNewId: createNewId);
            return tableId;
        }


        [HttpDelete]
        [Route("ClearAllData")]
        public async Task ClearAllData()
        {
            await _csvImportService.ClearAllData();
        }
    }
}