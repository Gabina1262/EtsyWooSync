using EtsyWooSync.Inerface;
using EtsyWooSync.Models;

namespace EtsyWooSync.Services
{
   public class ProductSetService
    {
        public int GetAvailableStock(ProductSet set, List<IProduct> allProducts)
        {
            List<int> availableQuantities = new();

            foreach (int componentId in set.ProductsIdsInSet)
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

            int availableSets = GetAvailableStock(set, allProducts);

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
            set.Stock = GetAvailableStock(set, allProducts);

            Console.WriteLine($"Zpracována objednávka {quantity}x {set.Name}. Nový sklad: {set.Stock}");
        }
    }
}

