﻿namespace EtsyWooSync.Services;

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



        var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            MaxResponseHeadersLength = 128 // zvýší limit ze 64 KB na 128 KB
        };
        client = new HttpClient(handler);
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

    public async Task<bool> UpdateProductStockAsync(int id, int stock)
    {
        IConfiguration config = new ConfigurationBuilder()
          .AddUserSecrets<Program>()
          .Build();

        var apiUrl = config["WooCommerce:ApiUrl"] + $"/products/{id}";

        Console.WriteLine($"{id} → aktualizace skladu na {stock} ks");


        var responseGet = await client.GetAsync(apiUrl);

        if (!responseGet.IsSuccessStatusCode)
        {
            Console.WriteLine($"Chyba při volání produktu {id}: {responseGet.StatusCode}");
            return false;
        }

        var responseContent = await responseGet.Content.ReadAsStringAsync();

        using JsonDocument doc = JsonDocument.Parse(responseContent);
        bool manageStock = false;

        var productData = new Dictionary<string, object>
        {
              { "stock_quantity", stock }
        };

        if (!manageStock)
        {
            productData["manage_stock"] = true;
        }

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(productData),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await client.PutAsync(apiUrl, jsonContent);
            Console.WriteLine($"STATUS: {response.StatusCode}");
            return response.IsSuccessStatusCode;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při aktualizaci produktu {id}: {ex.Message}");
            return false;
        }
    }

    public async Task<List<JsonElement>> GetOrdersAsync()
    {
        var ordersUrl = apiUrl.Replace("/products", "/orders?per_page=10&orderby=date&order=desc");
        var response = await client.GetAsync(ordersUrl);
        var result = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Chyba API: {response.StatusCode} \n{result}");
            return new List<JsonElement>();
        }
        using JsonDocument doc = JsonDocument.Parse(result);
        var root = doc.RootElement;
        var orders = new List<JsonElement>();
        foreach (var order in root.EnumerateArray())
        {
            orders.Add(order);
        }
        return orders;
    }
    public async Task<List<JsonElement>> GetTodaysOrdersAsync()
    {
        var ordersUrl = apiUrl.Replace("/products", "/orders?per_page=50&orderby=date&order=desc");
        var response = await client.GetAsync(ordersUrl);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Chyba při načítání objednávek: {response.StatusCode}\n{error}");
            return new List<JsonElement>();
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var today = DateTime.UtcNow.Date.AddDays(-2);
        var todaysOrders = new List<JsonElement>();

        foreach (var order in root.EnumerateArray())
        {
            if (order.TryGetProperty("date_created_gmt", out var createdElement))
            {
                if (DateTime.TryParse(createdElement.GetString(), out var createdAtUtc))
                {
                    if (createdAtUtc.Date == today)
                    {
                        var raw = order.GetRawText();
                        todaysOrders.Add(JsonDocument.Parse(raw).RootElement);
                    }

                }

            }
        }
        return todaysOrders;
    }
}
