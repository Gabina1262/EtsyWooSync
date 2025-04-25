// See https://aka.ms/new-console-template for more information
using EtsyWooSync.Models;
using EtsyWooSync.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

class Program
{
    static async Task Main()

    {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<Program>();

        var config = builder.Build();
        var wooClient = new WooApiClient(config);
        var orderProcessor = new OrderProcessor();    

        var allProducts = await wooClient.GetAllProductsAsync();
        var snapshot = ProductSnapshotExporter.CreateFromProducts(allProducts);
        await ProductSnapshotExporter.ExportToJsonAsync(snapshot);

        var todaysOrders = await wooClient.GetTodaysOrdersAsync();
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

                Console.WriteLine($" Produkt: {name} (ID: {productId}, Varianta: {variationId}) x {quantity} ks – celkem: {itemTotal}");
            }

        }


        foreach (var order in todaysOrders)
        {
            if (order.TryGetProperty("line_items", out var lineItemsArray) && lineItemsArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var lineItem in lineItemsArray.EnumerateArray())
                {
                    int productId = lineItem.TryGetProperty("product_id", out var idElement) && idElement.ValueKind == JsonValueKind.Number
                        ? idElement.GetInt32()
                        : -1;

                    int? variationId = lineItem.TryGetProperty("variation_id", out var variationElement) && variationElement.ValueKind == JsonValueKind.Number
                        ? variationElement.GetInt32()
                        : (int?)null;

                    int quantity = lineItem.TryGetProperty("quantity", out var quantityElement) && quantityElement.ValueKind == JsonValueKind.Number
                        ? quantityElement.GetInt32()
                        : 0;

                    var product = allProducts.FirstOrDefault(p => p.WooId == productId);

                    if (product != null)
                    {
                        orderProcessor.ProcessOrderItem(product, quantity, allProducts, variationId);

                        if (product is Coin coin)
                        {
                            await wooClient.UpdateProductStockAsync(coin.WooId, coin.WholeBunch);
                        }
                        else if (product is ProductSet set)
                        {
                            await wooClient.UpdateProductStockAsync(set.WooId, set.Stock);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Produkt s ID {productId} nenalezen mezi produkty.");
                    }
                }
            }
        }


        //var update = await client.UpdateProductStockAsync(4674, 1990);
        //var product = await client.GetProductsAsync();

        //foreach (var item in product)
        //{
        //    Console.WriteLine($" Id: {item.id} Název: {item.name}, Skladem: {item.stock}");
        //}
        //Console.WriteLine($"Načteno {product.Count} produktů.");
        //Console.ReadLine();

      



        Console.ReadLine();
    }
}
