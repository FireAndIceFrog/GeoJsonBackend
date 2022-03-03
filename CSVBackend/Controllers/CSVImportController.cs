using CSVBackend.services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Text.Json;

namespace CSVBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("MyPolicy")]
    public class CSVImportController : ControllerBase
    {
        
        private readonly ILogger<CSVImportController> _logger;
        private readonly ICSVImportService _csvImportService;

        public CSVImportController(ILogger<CSVImportController> logger, ICSVImportService csvImporter)
        {
            _logger = logger;
            _csvImportService = csvImporter;
        }

        [HttpGet]
        [Route("GetWeeklyData")]
        public async Task<string> GetWeeklyData(int partNumber = 0)
        {
            var response = await _csvImportService.GetWeeklyDataAsync(partNumber);
            return response;
        }

        [HttpPost]
        [Route("SetWeeklyData")]
        public async Task SetWeeklyDataAsync([FromBody] object weeklyData)
        {
            await _csvImportService.SetWeeklyDataAsync((JsonElement)weeklyData);
        }

        [HttpPut]
        [Route("UpdateData")]
        public async Task UpdateWeeklyDataAsync([FromBody] object weeklyData, int partNumber)
        {
            await _csvImportService.UpdateWeeklyDataAsync((JsonElement)weeklyData, partNumber);
        }

        [HttpDelete]
        [Route("ClearAllData")]
        public async Task ClearAllData()
        {
            await _csvImportService.ClearAllData();
        }

        [HttpGet]
        [Route("Count")]
        public async Task<int> CountAllData()
        {
            return await _csvImportService.CountAllData();
        }
    }
}