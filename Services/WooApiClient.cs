namespace EtsyWooSync.Services;

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class WooApiClient
{
    private readonly HttpClient client;
    private readonly string apiUrl;

    public WooApiClient(IConfiguration config)
    {

        string consumerKey = config["WooCommerce:ConsumerKey"];
        string consumerSecret = config["WooCommerce:ConsumerSecret"];
        apiUrl = config["WooCommerce:ApiUrl"] + "/products";


        client = new HttpClient();

        var byteArray = Encoding.ASCII.GetBytes($"{consumerKey}:{consumerSecret}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
    }
    public async Task<List<(int id, string name, int? stock)>> GetProductsAsync()
    {

        try
        {
            var response = await client.GetAsync(apiUrl);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API chyba: {response.StatusCode} \n{result}");

            var list = new List<(int id, string name, int? stock)>();

            using JsonDocument doc = JsonDocument.Parse(result);
            foreach (var product in doc.RootElement.EnumerateArray())
            {
                int id = product.TryGetProperty("id", out var idElement) &&
           idElement.ValueKind == JsonValueKind.Number
                  ? idElement.GetInt32()
                    : -1;

                string name = product.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String
                    ? nameElement.GetString()!
                    : "(bez názvu)";

                int? stock = product.TryGetProperty("stock_quantity", out var stockElement) && stockElement.ValueKind != JsonValueKind.Null
                  ? stockElement.GetInt32()
                  : (int?)null;
               
                list.Add((id, name, stock));


            }

            return list;

        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Chyba při volání HTTP: {ex.Message}");

            return new List<(int id, string name, int? stock)>();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Obecná chyba: {ex.Message}");

            return new List<(int id, string name, int? stock)>();

        }

    }
    public async Task<(string name, int? stock)?> GetProductByIdAsync(int id)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var apiUrl = config["WooCommerce:ApiUrl"] + $"/products/{id}";

        try
        {
            var response = await client.GetAsync(apiUrl);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Chyba při volání produktu {id}: {response.StatusCode}");
                return null;
            }

            using JsonDocument doc = JsonDocument.Parse(result);
            var root = doc.RootElement;

            string name = root.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String
                ? nameElement.GetString()!
                : "(bez názvu)";

            int? stock = root.TryGetProperty("stock_quantity", out var stockElement) && stockElement.ValueKind != JsonValueKind.Null
                ? stockElement.GetInt32()
                : (int?)null;

            return (name, stock);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při volání produktu {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<List<(string name, int? stock)>> GetAllProductsAsync()
    {
        IConfiguration config = new ConfigurationBuilder()
          .AddUserSecrets<Program>()
          .Build();


        var allProducts = new List<(string name, int? stock)>();
        var page = 1;
        while (true)
        {

            var apiUrl = config["WooCommerce:ApiUrl"] + $"/products?per_page=100&page={page}";
            var response = await client.GetAsync(apiUrl);
           var result = await response.Content.ReadAsStringAsync(); 
            Console.WriteLine($"URL: {apiUrl}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Chyba API: {response.StatusCode} \n{result}");
                break;
            }

            using JsonDocument doc = JsonDocument.Parse(result);
            var root = doc.RootElement;

            if (root.GetArrayLength() == 0)
                break;

            foreach (var product in root.EnumerateArray())
            {
                string name = product.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String
                    ? nameElement.GetString()!
                    : "(bez názvu)";

                int? stock = product.TryGetProperty("stock_quantity", out var stockElement) && stockElement.ValueKind != JsonValueKind.Null
                    ? stockElement.GetInt32()
                    : (int?)null;

                allProducts.Add((name, stock));
            }

            page++;
        }

        return allProducts;

    }
}
