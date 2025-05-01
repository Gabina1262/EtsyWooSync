using EtsyWooSync.Inerface;
using EtsyWooSync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EtsyWooSync.Services;

public class StockResetService
{
    private readonly IWooApiClient wooClient;
    public StockResetService(IWooApiClient wooClient)
    {
        this.wooClient = wooClient;
    }

    public async Task RunInitialStockResetFromSnapshotAsync(List<ProductSnapshot> snapshot)
    {
        foreach (var product in snapshot)
        {
            if (!product.Name.Contains("mince", StringComparison.OrdinalIgnoreCase) &&
                !product.Name.Contains("coin", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Console.WriteLine($"\n🔄 Přepočítávám produkt: {product.Name} (ID: {product.Id})");
            Console.WriteLine($"🧮 WholeBunch zásoba: {product.TotalStock}");

            var variants = await wooClient.GetVariantsForProductAsync(product.Id);

            if (variants.Count == 0)
            {
                Console.WriteLine("⚠️ Žádné varianty – přeskakuji.");
                continue;
            }

            var stockByColor = new Dictionary<string, int>();

            if (product.Attributes != null &&
                product.Attributes.TryGetValue("Color", out var colors) &&
                colors.Count > 0)
            {
                foreach (var color in colors)
                {
                    var key = color.Trim();
                    Console.WriteLine($"🎨 Detekovaná barva: {key}");
                    stockByColor[key] = product.TotalStock;
                }
            }
            else
            {
                Console.WriteLine($"🎨 Žádná barva – použita výchozí '__default__'");
                stockByColor["__default__"] = product.TotalStock;
            }

            Console.WriteLine("📦 Načtené varianty:");
            foreach (var v in variants)
            {
                Console.WriteLine($"   ID {v.VariantId} | barva: {v.Color} | balení: {v.QuantityPerPackage}");
            }

            var updates = StockDistributor.DistributeStockWithOptionalColor(stockByColor, variants);

            if (updates.Count == 0)
            {
                Console.WriteLine("⚠️ Žádné varianty k aktualizaci – přeskakuji.");
                continue;
            }

            foreach (var update in updates)
            {
                Console.WriteLine($"⬆️  Aktualizuji variantu {update.VariantId} → nový sklad: {update.NewStockQuantity}");
                var success = await wooClient.UpdateVariantStockAsync(product.Id, update.VariantId, update.NewStockQuantity);
                if (!success)
                {
                    Console.WriteLine($"❌ Nezdařilo se aktualizovat variantu {update.VariantId}");
                }
            }

            Console.WriteLine("✅ Přepočet dokončen.");
        }

        Console.WriteLine("\n🎉 HOTOVO – Debug přepočet dokončen.");
    }
}