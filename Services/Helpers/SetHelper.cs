using EtsyWooSync.Inerface;
using EtsyWooSync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtsyWooSync.Services
{
    namespace EtsyWooSync.Services
    {
        public static class SetHelper
        {
            public static int CalculateSetStock(GenericSet set, List<IProduct> allProducts)
            {
                // Pokud nemáme žádné produkty v setu, není skladem vůbec nic.
                if (set.ProductsIdsInSet == null || set.ProductsIdsInSet.Count == 0)
                    return 0;

                // Pro každý produkt v setu najdeme jeho aktuální sklad a zjistíme nejmenší hodnotu.
                var stocks = new List<int>();

                foreach (var productId in set.ProductsIdsInSet)
                {
                    var product = allProducts.FirstOrDefault(p => p.WooId == productId);

                    if (product != null)
                    {
                        stocks.Add(product.Stock);
                    }
                    else
                    {
                        // Pokud produkt v seznamu chybí, bereme to jako 0 (není skladem).
                        stocks.Add(0);
                    }
                }

                if (stocks.Count == 0)
                    return 0;

                // Dostupný počet setů = minimum ze skladových zásob komponent
                return stocks.Min();
            }
        }
    }

}
