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


        //var update = await client.UpdateProductStockAsync(4674, 1990);
        //var product = await client.GetProductsAsync();

        //foreach (var item in product)
        //{
        //    Console.WriteLine($" Id: {item.id} Název: {item.name}, Skladem: {item.stock}");
        //}
        //Console.WriteLine($"Načteno {product.Count} produktů.");
        //Console.ReadLine();

        var todaysOrders = await client.GetTodaysOrdersAsync();
        Console.WriteLine($"Dnešní objednávky ({todaysOrders.Count}):");
        foreach (var order in todaysOrders)
        {
            var id = order.GetProperty("id").GetInt32();
            var date = order.GetProperty("date_created_gmt").GetString();
            var status = order.GetProperty("status").GetString();
            var total = order.GetProperty("total").GetString();

            Console.WriteLine($"\nObjednávka #{id} ({status}), vytvořena: {date}, cena celkem: {total}");

            var lineItems = order.GetProperty("line_items");
            foreach (var item in lineItems.EnumerateArray())
            {
                var productId = item.GetProperty("product_id").GetInt32();
                var variationId = item.GetProperty("variation_id").GetInt32(); // = 0 pokud není
                var name = item.GetProperty("name").GetString();
                var quantity = item.GetProperty("quantity").GetInt32();
                var itemTotal = item.GetProperty("total").GetString();

                Console.WriteLine($"  🛒 Produkt: {name} (ID: {productId}, Varianta: {variationId}) x {quantity} ks – celkem: {itemTotal}");
            }
           
        }
        Console.ReadLine();
    }
}
