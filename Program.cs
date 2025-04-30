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

        var todaysOrders = await wooClient.GetTodaysOrdersAsync();

        foreach (var order in todaysOrders)
        {
            Console.WriteLine("============== OBJEDNÁVKA ==============");

            if (order.TryGetProperty("id", out var orderId))
            {
                Console.WriteLine($"Objednávka ID: {orderId}");
            }

            if (order.TryGetProperty("line_items", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    string name = item.GetProperty("name").GetString();
                    int productId = item.GetProperty("product_id").GetInt32();
                    int quantity = item.GetProperty("quantity").GetInt32();

                    // variant_id může být 0 pokud jde o produkt bez varianty
                    int variationId = item.TryGetProperty("variation_id", out var varId) ? varId.GetInt32() : 0;

                    Console.WriteLine($"→ {name}");
                    Console.WriteLine($"   Product ID: {productId}");
                    Console.WriteLine($"   Variation ID: {variationId}");
                    Console.WriteLine($"   Quantity: {quantity}");

                    // Volitelně: výpis meta dat (např. barva, balení atd.)
                    if (item.TryGetProperty("meta_data", out var meta))
                    {
                        foreach (var metaItem in meta.EnumerateArray())
                        {
                            string key = metaItem.GetProperty("key").GetString();
                            string value = metaItem.GetProperty("value").ToString(); // může být číslo nebo JSON

                            Console.WriteLine($"   · {key}: {value}");
                        }
                    }



                    //int productId = 4674;

                    //await TestLoadingVariants(wooClient, productId);

                    Console.ReadLine();
                }
            }
        }
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



