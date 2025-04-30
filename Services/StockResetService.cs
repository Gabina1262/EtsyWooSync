using EtsyWooSync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EtsyWooSync.Services
{
    class StockResetService
    {
        private readonly WooApiClient wooClient;

        public async Task RunInitialStockResetFromSnapshotAsync(List<ProductSnapshot> snapshot)
        {
            foreach (var product in snapshot)
            {
                // 1. Filtruj jen mince podle názvu
                if (!product.Name.Contains("mince", StringComparison.OrdinalIgnoreCase) &&
                    !product.Name.Contains("coin", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Console.WriteLine($"🔄 Přepočítávám produkt: {product.Name} (ID: {product.Id})");

                // 2. Načti varianty produktu z Woo
                var allVariants = await wooClient.GetVariantsForProductAsync(product.Id);
                if (allVariants.Count == 0)
                {
                    Console.WriteLine("Žádné varianty – přeskakuji.");
                    continue;
                }

                // 3. Vytvoř slovník wholeBunch zásob podle barvy
               
                var stockByColor = new Dictionary<string, int>();

                // Pokud má produkt barvy → přepočítáme každou zvlášť
                if (product.Attributes != null &&
                    product.Attributes.TryGetValue("Color", out var colors) &&
                    colors != null &&
                    colors.Count > 0)
                {
                    foreach (var color in colors)
                    {
                        if (!string.IsNullOrWhiteSpace(color))
                        {
                            var key = color.Trim();
                            stockByColor[key] = product.TotalStock;
                        }
                    }
                }
                else
                {
                    // Produkt nemá barvy → použij výchozí klíč
                    stockByColor["__default__"] = product.TotalStock;
                }

                // 4. Přepočítej nové zásoby variant
                var processor = new CoinOrderProcessor(wooClient);
                var updates = StockDistributor.DistributeStockWithOptionalColor(stockByColor, allVariants);

                // 5. Odešli update každé varianty do Woo
                foreach (var update in updates)
                {
                    await wooClient.UpdateVariantStockAsync(product.Id, update.VariantId, update.NewStockQuantity);
                }

                Console.WriteLine($"Hotovo: {product.Name}");
            }

            Console.WriteLine("Všechny mince přepočítány.");
        }
    }
}
