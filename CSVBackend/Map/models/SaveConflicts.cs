namespace CSVBackend.Map.models
{
    public class SaveConflicts
    {
        public GeoJsonModel<dynamic> before { get; set; }
        public GeoJsonModel<dynamic> after { get; set; }
    }
}
