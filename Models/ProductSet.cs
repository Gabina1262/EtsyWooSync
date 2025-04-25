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
        public  List<string> Categories { get; set; } = new(); // seznam kategorií
        public  List<string> Tags { get; set; } = new();
        public Dictionary<string, List<string>> Attributes { get; set; } = new(); // seznam atributů

        public int GetAvailableStock(List<IProduct> allProducts)
        {
            List<int> availableQuantities = new();

            foreach (int componentId in ProductsIdsInSet)
            {
                var product = allProducts.FirstOrDefault(p => p.WooId == componentId);

                if (product == null)
                {
                    Console.WriteLine($"Komponenta s ID {componentId} nebyla nalezena ve skladu.");
                    return 0;
                }

                availableQuantities.Add(product.Stock);
            }

            return availableQuantities.Min();
        }

        public void ProcessSetOrder(int setId, int quantity, List<IProduct> allProducts)
        {
            var set = allProducts.OfType<ProductSet>().FirstOrDefault(s => s.WooId == setId);

            if (set == null)
            {
                Console.WriteLine($"Set s ID {setId} nebyl nalezen.");
                return;
            }

            int availableSets = set.GetAvailableStock(allProducts);

            if (availableSets < quantity)
            {
                Console.WriteLine($"Nedostatek komponent pro objednávku {quantity}x {set.Name}. Dostupné pouze {availableSets} ks.");
                return;
            }

            // Odečíst X kusů každé komponenty
            foreach (int componentId in set.ProductsIdsInSet)
            {
                var component = allProducts.FirstOrDefault(p => p.WooId == componentId);

                if (component != null)
                    switch (component)
                    {
                        case Product p:
                            p.Stock -= quantity;
                            break;

                        case ProductSet ps:
                            ps.Stock -= quantity;
                            break;

                        default:
                            Console.WriteLine($"Produkt typu {component.GetType().Name} nelze upravit (nemá veřejný setter Stock).");
                            break;
                    }
            }

            // Aktualizovat dostupnost setu
            set.Stock = set.GetAvailableStock(allProducts);

            Console.WriteLine($"Zpracována objednávka {quantity}x {set.Name}. Nový sklad: {set.Stock}");
        }
    }

}