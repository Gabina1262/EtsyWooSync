namespace EtsyWooSync.Models
{
    public class ProductSnapshot
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "simple";
        public int TotalStock { get; set; }
        public List<int>? VariationIds { get; set; }
        public List<string>? Categories { get; set; }
        public List<string>? Tags { get; set; }
        public Dictionary<string, List<string>>? Attributes { get; set; }
    }
}
