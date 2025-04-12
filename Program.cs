// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

class Program
{
    static async Task Main()

    {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<Program>();

        IConfiguration config = builder.Build();

        string consumerKey = config["WooCommerce:ConsumerKey"];
        string consumerSecret = config["WooCommerce:ConsumerSecret"];
        string url = config["WooCommerce:ApiUrl"] + "/products";
       
        using var client = new HttpClient();

        var byteArray = Encoding.ASCII.GetBytes($"{consumerKey}:{consumerSecret}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        try
        {


            var response = await client.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using JsonDocument doc = JsonDocument.Parse(result);
                foreach (var product in doc.RootElement.EnumerateArray())
                {
                    string name;

                    if (product.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String)
                    {
                        name = nameElement.GetString()!;
                    }
                    else
                    {
                        name = "(bez názvu)";
                    }
                    int? stock = product.TryGetProperty("stock_quantity", out var stockElement) && stockElement.ValueKind != JsonValueKind.Null
                        ? stockElement.GetInt32()
                        : (int?)null;

                    Console.WriteLine($"{name} — skladem: {stock}");
                }
            }
            else
            {
                Console.WriteLine($"Chyba: {response.StatusCode}");
                Console.WriteLine(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při nastavení hlavičky: {ex.Message}");
            return;
        }
    }
}

