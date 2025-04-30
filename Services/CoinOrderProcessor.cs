using EtsyWooSync.Inerface;
using EtsyWooSync.Models;
using System.Text.Json;

namespace EtsyWooSync.Services
{

    public class CoinOrderProcessor
    {
        private readonly IWooApiClient wooClient;
               
        public CoinOrderProcessor(IWooApiClient wooClient)
        {
            this.wooClient = wooClient;
        }

      

        public async Task HandleCoinOrderItem(JsonElement lineItem, Dictionary<string, int> wholeBunchStock)
        {
            int productId = lineItem.GetProperty("product_id").GetInt32();
            int variantId = lineItem.GetProperty("variation_id").GetInt32();
            int quantity = lineItem.GetProperty("quantity").GetInt32();

            // Výchozí hodnoty
            string? color = null;
            int quantityPerPackage = 1;

            if (lineItem.TryGetProperty("meta_data", out var meta))
            {
                foreach (var metaItem in meta.EnumerateArray())
                {
                    var key = metaItem.GetProperty("key").GetString();
                    var value = metaItem.GetProperty("value").ToString();

                    if (key == "pa_color")
                        color = value;
                    else if (key == "pa_mnozstvi-v-baleni")
                    {
                        if (int.TryParse(value.Split('-')[0], out int q))
                            quantityPerPackage = q;
                    }
                }
            }

            var allVariants = await wooClient.GetVariantsForProductAsync(productId);

            // Pojistný přepočet skladu
            string stockKey = string.IsNullOrWhiteSpace(color) ? "__default__" : color;
            if (wholeBunchStock.TryGetValue(stockKey, out int wholeBunch))
            {
                
                var updates = StockDistributor.DistributeStockWithOptionalColor(wholeBunchStock, allVariants);

                foreach (var update in updates)
                {
                    await wooClient.UpdateVariantStockAsync(productId, update.VariantId, update.NewStockQuantity);
                }
            }
            else
            {
                Console.WriteLine($"Chybí wholeBunch zásoba pro barvu {stockKey}");
            }

            // Odečíst jednotky z variant
            int totalCoins = quantity * quantityPerPackage;
            Console.WriteLine($"Odečítám {totalCoins} mincí pro variantu {variantId}");

            int remaining = totalCoins;

            foreach (var v in allVariants.OrderByDescending(v => v.QuantityPerPackage))
            {
                int count = remaining / v.QuantityPerPackage;
                remaining %= v.QuantityPerPackage;

                if (count > 0)
                {
                    Console.WriteLine($"➖ -{count} balení × {v.QuantityPerPackage} ks → variant ID {v.VariantId}");
                    // POZOR: tady by ses musela nejdřív zeptat Woo na aktuální stav skladu
                    // a pak poslat nový přepočítaný stav
                    // např. await wooClient.UpdateVariantStockAsync(productId, v.VariantId, newQty);
                }
            }

            // Odečtení z wholeBunch v paměti
            wholeBunchStock[stockKey] -= totalCoins;
        }

    }
}
