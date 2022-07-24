using System.Text.Json;

namespace CSVBackend.Map.Services;

public interface IMapLayerService
{
    public Task<string> GetFeatures(double x, double y, double z);

    public Task CreateFeature(object data);
    public Task<string> UpdateFeatures(object data);
    public Task DeleteFeaturesAsync(object data);
    public Task<bool> ClearAllData();
}
