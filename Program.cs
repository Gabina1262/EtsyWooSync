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
            .AddUserSecrets<Program>()  // důležité!!!
            .Build();

        var wooClient = new WooApiClient(config);

        int productId = 4674;

        await TestLoadingVariants(wooClient, productId);

        Console.ReadLine();
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


