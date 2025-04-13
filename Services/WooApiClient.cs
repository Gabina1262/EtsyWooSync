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
    public async Task<List<(string name, int? stock)>> GetProductsAsync()
    {
        Console.WriteLine($"URL: {apiUrl}");
        try
        {
            var response = await client.GetAsync(apiUrl);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API chyba: {response.StatusCode} \n{result}");

            var list = new List<(string name, int? stock)>();

            using JsonDocument doc = JsonDocument.Parse(result);
            foreach (var product in doc.RootElement.EnumerateArray())
            {
                string name = product.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String
                    ? nameElement.GetString()!
                    : "(bez názvu)";

                int? stock = product.TryGetProperty("stock_quantity", out var stockElement) && stockElement.ValueKind != JsonValueKind.Null
                  ? stockElement.GetInt32()
                  : (int?)null;
                list.Add((name, stock));
               
            }
           
            return list;
           
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Chyba při volání HTTP: {ex.Message}");
          
            return new List<(string name, int? stock)>();
           
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Obecná chyba: {ex.Message}");
           
            return new List<(string name, int? stock)>();
           
        }

    }
    public async Task GetProductByIdAsync(string id)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        string consumerKey = config["WooCommerce:ConsumerKey"];
        string consumerSecret = config["WooCommerce:ConsumerSecret"];
        string url = config["WooCommerce:ApiUrl"] + $"/products/{id}";

        var response = await client.GetAsync(url);
        var result = await response.Content.ReadAsStringAsync();    
        if (!response.IsSuccessStatusCode)
            throw new Exception($"API chyba: {response.StatusCode} \n{result}");

        using JsonDocument doc = JsonDocument.Parse(result);    


    }

    public async Task<List<(string name, int? stock)>> GetAllProductsAsync()
    {
        var listProducts = new List<(string name, int? stock)>();
        var page = 1;
        
        while (true)
        { 
        var allProducts = await GetProductsAsync();
        }


    }
}
