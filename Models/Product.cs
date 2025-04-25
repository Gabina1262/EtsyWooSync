using EtsyWooSync.Inerface;

namespace EtsyWooSync.Models
{
    public class Product : IProduct, IHasCategories, IHasTags, IHasAttributes
    {
        public int WooId { get; set; } = 0; // ID produktu ve WooCommerce
        public string Name { get; set; } = string.Empty; // Název produktu
        public int Stock { get; set; } = 0; // Počet kusů skladem

        // Implementace rozhraní IHasCategories
        public List<string> Categories { get; set; } = new();

        // Implementace rozhraní IHasTags
        public List<string> Tags { get; set; } = new();

        // Implementace rozhraní IHasAttributes
        public Dictionary<string, List<string>> Attributes { get; set; } = new();
    }
}