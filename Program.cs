// See https://aka.ms/new-console-template for more information
using EtsyWooSync.Inerface;
using EtsyWooSync.Models;
using EtsyWooSync.Services;
using EtsyWooSync.Services.EtsyWooSync.Services;
using EtsyWooSync.Services.Helpers;
using EtsyWooSync.Services.StockReset;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using static Coin;

class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
     .AddUserSecrets<Program>()
     .Build();

        var wooClient = new WooApiClient(config);
        var coinSnapshot = await ProductSnapshotExporter.GetSnapshotsAsync();
        var allProducts = await wooClient.GetAllProductsAsync();

        // var coinReset = new CoinStockReset(wooClient);
        var setReset = new SetStockReset();
        // var resetter = new GeneralStockReset(coinReset, setReset);

        // Výpis před přepočtem
        Console.WriteLine("=== SETY PŘED PŘEPOČTEM ===");
        var setsBefore = allProducts.OfType<ProductSet>()
            .Select(s => new { s.WooId, s.Name, Stock = s.Stock })
            .ToList();
        foreach (var s in setsBefore)
        {
            Console.WriteLine($"- {s.Name} (ID {s.WooId}) → Sklad: {s.Stock}");
        }

        // Přepočet
        setReset.ResetSetStockAsync(allProducts);

        // Výpis po přepočtu
        Console.WriteLine("=== SETY PO PŘEPOČTU ===");
        var setsAfter = allProducts.OfType<ProductSet>().ToList();
        foreach (var set in setsAfter)
        {
            Console.WriteLine($"- {set.Name} (ID {set.WooId}) → Nový sklad: {set.Stock}");
        }

        // Výpis změn
        Console.WriteLine("=== SETY, KTERÉ SE ZMĚNILY ===");
        foreach (var s in setsBefore)
        {
            var updated = setsAfter.FirstOrDefault(p => p.WooId == s.WooId);
            if (updated != null && updated.Stock != s.Stock)
            {
                Console.WriteLine($"✓ {s.Name} (ID {s.WooId}) → {s.Stock} → {updated.Stock}");
            }
        }
    }
}


   



