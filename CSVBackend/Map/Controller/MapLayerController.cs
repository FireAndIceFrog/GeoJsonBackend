using CSVBackend.Ryan.services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Models;
using System.Text.Json;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using CSVBackend.Map.Services;

namespace CSVBackend.Controllers
{
    [ApiController]
    [Route("MapLayers")]
    [EnableCors("MyPolicy")]
    public class MapLayerController : ControllerBase
    {
        
        private readonly ILogger<MapLayerController> _logger;
        private readonly IMapLayerService _mapLayer;

        public MapLayerController(ILogger<MapLayerController> logger, IMapLayerService mapLayer)
        {
            _logger = logger;
            _mapLayer = mapLayer;
        }

        [HttpPost]
        [Route("CreateFeature")]
        public async Task CreateFeature([FromBody] object data)
        {
            await _mapLayer.CreateFeature(data);
        }

        [HttpGet]
        [Route("GetFeature")]
        public async Task<string> GetFeature(double x, double y, double z)
        {
            return await _mapLayer.GetFeatures(x, y, z);
        }
    }
}