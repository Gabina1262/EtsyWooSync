using EtsyWooSync.Inerface;
using EtsyWooSync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtsyWooSync.Services
{
   public class OrderProcessor
    {
        public void ProcessOrderItem(IProduct product, int quantity, List<IProduct> allProducts, int? variationId = null)
        {
            switch (product)
            {
                case ProductSet set:
                    set.ProcessSetOrder(set.WooId, quantity, allProducts);
                    break;

                case Coin coin:
                    if (variationId.HasValue)
                    {
                        coin.ProcessCoinOrder(variationId.Value, quantity);
                    }
                    else
                    {
                        Console.WriteLine($"Chybí variationId pro minci {coin.Name}");
                    }
                    break;

                // Produkt ignorujeme – Woo už ho zpracovalo.
                case Product:
                    Console.WriteLine($"WooCommerce zpracovalo objednávku pro {product.Name}, není třeba nic odečítat.");
                    break;

                default:
                    Console.WriteLine($"Neznámý typ produktu: {product.GetType().Name}");
                    break;
            }
        }
    }
}
