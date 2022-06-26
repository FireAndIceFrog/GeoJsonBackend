namespace CSVBackend.Map.models
{
    public class GeoJsonModelList<T>
    {
        public List<GeoJsonModel<T>>? features { get; set; }
    }

    public class GeoJsonModel<T>
    {
        public string? _id { get; set; }
        public string? type { get; set; }
        public GeoJsonGeometry<T>? geometry { get; set; }
        public GeoJsonProperties? properties { get; set; }
    }

    public class GeoJsonGeometry<T>
    {
        public string? type { get; set; }
        public IEnumerable<IEnumerable<T>>? coordinates { get; set; }
    }

    public class GeoJsonProperties 
    { 
        public string? id { get; set; }
        public string? backgroundColor { get; set; }
        public string? borderColor { get; set; }
    }
}
