using EtsyWooSync.Inerface;

namespace EtsyWooSync.Models
{
    public abstract class ProductSet : IProduct, IHasCategories, IHasTags, IHasAttributes
    {
        public int WooId { get; set; } = 0; // ID produktu na WooCommerce   
        public string Name { get; set; } = string.Empty; // název setu

        public int Stock { get; set; } = 0; // počet kusů v setu
        public List<int> ProductsIdsInSet { get; set; } = new List<int>(); // ID produktů v setu
        public bool IsAvalable { get; set; } = true;
        public List<string> Categories { get; set; } = new(); // seznam kategorií
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, List<string>> Attributes { get; set; } = new(); // seznam atributů

        internal void ProcessSetOrder(int wooId, int quantity, List<IProduct> allProducts)
        {
            throw new NotImplementedException();
        }
    }

}