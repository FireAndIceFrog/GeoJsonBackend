using CSVBackend.services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CSVBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        public async Task<string> GetWeeklyData()
        {
            var data = await _csvImportService.GetWeeklyDataAsync();
            string jsonresp = JsonConvert.SerializeObject(data);
            return jsonresp;
        }

        [HttpPost]
        [Route("SetWeeklyData")]
        public async Task SetWeeklyDataAsync([FromBody] string weeklyData)
        {
            await _csvImportService.SetWeeklyDataAsync(weeklyData);
        }
    }
}