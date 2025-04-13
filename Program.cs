// See https://aka.ms/new-console-template for more information
using EtsyWooSync.Services;
using Microsoft.Extensions.Configuration;

class Program
{
    static async Task Main()

    {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<Program>();

        var config = builder.Build();
        var client = new WooApiClient(config);

        var product = await client.GetProductsAsync();
        foreach (var item in product)
        {
            Console.WriteLine($"Název: {item.name}, Skladem: {item.stock}");
        }
        Console.WriteLine($"Načteno {product.Count} produktů.");
        Console.ReadLine(); 
    }
}

