// See https://aka.ms/new-console-template for more information
using EtsyWooSync.Inerface;
using EtsyWooSync.Models;
using EtsyWooSync.Services;
using EtsyWooSync.Services.EtsyWooSync.Services;
using EtsyWooSync.Services.Helpers;
using Microsoft.Extensions.Configuration;
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
        var wooClientInterface = (IWooApiClient)wooClient;

        // 🔁 Stáhni snapshot z Woo a ulož do souboru
        await ProductSnapshotExporter.CreateFromWooAsync(wooClient);

        // 📂 Načti snapshot
        var snapshot = await ProductSnapshotExporter.GetSnapshotsAsync();

        if (snapshot.Count == 0)
        {
            Console.WriteLine("❌ Snapshot je prázdný. Ukončuji.");
            return;
        }

        Console.WriteLine($"\n🔧 Přepočítávám sklad pro {snapshot.Count} produktů...\n");

        var stockReset = new StockResetService(wooClientInterface);

        await stockReset.RunInitialStockResetFromSnapshotAsync(snapshot);

        Console.WriteLine("\n🎉 HOTOVO! Všechny mince byly aktualizovány.");
    }


    public static async Task TestLoadingVariants(WooApiClient wooClient, int productId)
    {
        // 1. Načteme ID variant
        List<int> variantIds = await wooClient.LoadVariantIdsAsync(productId);

        if (variantIds.Count == 0)
        {
            Console.WriteLine("Žádné varianty nebyly nalezeny.");
            return;
        }

        Console.WriteLine($"Nalezeno {variantIds.Count} variant:");
        foreach (var id in variantIds)
        {
            Console.WriteLine($"- {id}");
        }
    }
}



