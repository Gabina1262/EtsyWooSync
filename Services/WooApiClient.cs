namespace EtsyWooSync.Services;
using global::EtsyWooSync.Inerface;
using global::EtsyWooSync.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class WooApiClient
{
    private readonly HttpClient client;
    private readonly string baseApiUrl;


    public WooApiClient(IConfiguration config)
    {

        string consumerKey = config["WooCommerce:ConsumerKey"];
        string consumerSecret = config["WooCommerce:ConsumerSecret"];
        baseApiUrl = config["WooCommerce:ApiUrl"] ?? throw new ArgumentNullException("ApiUrl in config");




        var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            MaxResponseHeadersLength = 128 // zvýší limit ze 64 KB na 128 KB
        };
      
        client = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseApiUrl)
        };
       
        var byteArray = Encoding.ASCII.GetBytes($"{consumerKey}:{consumerSecret}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
    }
    public async Task<string> GetAsync(string relativeUrl)
    {

        var fullUrl = $"{baseApiUrl.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
        Console.WriteLine($"Requesting: {fullUrl}");

        try
        {
            var response = await client.GetAsync(fullUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($" Response Status: {response.StatusCode}");
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Content: {error}");
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při HTTP volání: {ex.Message}");
            return null;
        }
    }

    
    public async Task<List<(int id, string name, int? stock)>> GetProductsAsync()
    {

        try
        {
            var response = await client.GetAsync(baseApiUrl + "/products");
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


        var result = await GetAsync($"/products/{id}");

        try
        {
            var response = await client.GetAsync(baseApiUrl);

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

    public async Task<List<IProduct>> GetAllProductsAsync()
    {

        var allProducts = new List<IProduct>();
        var page = 1;
        while (true)
        {

            var result = await GetAsync(baseApiUrl) + $"/products?per_page=100&page={page}";

            using JsonDocument doc = JsonDocument.Parse(result);
            var root = doc.RootElement;

            if (root.GetArrayLength() == 0)
                break;

            foreach (var product in root.EnumerateArray())
            {
                int wooId = product.TryGetProperty("id", out var idElement) &&
                    idElement.ValueKind == JsonValueKind.Number
                    ? idElement.GetInt32()
                    : -1;

                string name = product.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String
                    ? nameElement.GetString()!
                    : "(bez názvu)";

                int? stock = product.TryGetProperty("stock_quantity", out var stockElement) && stockElement.ValueKind != JsonValueKind.Null
                    ? stockElement.GetInt32()
                    : (int?)null;

                List<string> categories = new();

                if (product.TryGetProperty("categories", out var catArray) && catArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var cat in catArray.EnumerateArray())
                    {
                        if (cat.TryGetProperty("name", out var catName) && catName.ValueKind == JsonValueKind.String)
                        {
                            categories.Add(catName.GetString()!);
                        }
                    }
                }

                List<string> tags = new();

                if (product.TryGetProperty("tags", out var tagArray) && tagArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var tag in tagArray.EnumerateArray())
                    {
                        if (tag.TryGetProperty("name", out var tagName) && tagName.ValueKind == JsonValueKind.String)
                        {
                            tags.Add(tagName.GetString()!);
                        }
                    }
                }

                Dictionary<string, List<string>> attributes = new();
                if (product.TryGetProperty("attributes", out var attrArray) && attrArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var attr in attrArray.EnumerateArray())
                    {
                        if (attr.TryGetProperty("name", out var attrName) && attrName.ValueKind == JsonValueKind.String)
                        {
                            string nameAttr = attrName.GetString()!;
                            List<string> values = new();
                            if (attr.TryGetProperty("options", out var optionsArray) && optionsArray.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var option in optionsArray.EnumerateArray())
                                {
                                    if (option.ValueKind == JsonValueKind.String)
                                    {
                                        values.Add(option.GetString()!);
                                    }
                                }
                            }
                            attributes[nameAttr] = values;
                        }
                    }
                }

                allProducts.Add(new Product
                {
                    WooId = wooId,
                    Name = name,
                    Stock = stock ?? 0,
                    Categories = categories,
                    Tags = tags,
                    Attributes = attributes
                }
                );
            }

            page++;
        }

        return allProducts;

    }

    public async Task<bool> UpdateProductStockAsync(int id, int? stock)
    {

        var updateUrl = $"{baseApiUrl}/products/{id}";

        Console.WriteLine($"{id} → aktualizace skladu na {stock} ks");

        var productData = new Dictionary<string, object>
        {
              { "stock_quantity", stock },
              { "manage_stock", true }

        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(productData),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await client.PutAsync(updateUrl, jsonContent);
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
        var response = await client.GetAsync($"{baseApiUrl}/orders?per_page=10&orderby=date&order=desc");

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

        var response = await client.GetAsync($"{baseApiUrl}/orders?per_page=50&orderby=date&order=desc");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Chyba při načítání objednávek: {response.StatusCode}\n{error}");
            return new List<JsonElement>();
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var today = DateTime.UtcNow.Date.AddDays(-1);
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

    public async Task<List<int>> LoadVariantIdsAsync(int productId)
    {

        var variationsId = new List<int>();

        var relativeUrl = $"products/{productId}/variations?per_page=100";

        var result = await GetAsync(relativeUrl);

        if (string.IsNullOrEmpty(result))
        {
            Console.WriteLine($"Varianta nebyla načtena (null/empty response) pro produkt {productId}");
            return variationsId;
        }

        Console.WriteLine($" Response Status: {result}");


        try
        {
            using JsonDocument doc = JsonDocument.Parse(result);
            foreach (var variation in doc.RootElement.EnumerateArray())
            {
                if (variation.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.Number)
                {
                    int variationId = idElement.GetInt32();
                    variationsId.Add(variationId);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při parsování variant: {ex.Message}");
        }

        return variationsId;
    }



    public async Task<List<Product>> LoadVariantDetailsAsync(List<int> variantIds)
    {
        var details = new List<Product>();

        foreach (var variantId in variantIds)
        {
            var relativeUrl = $"products/{variantId}";
            var result = await GetAsync(relativeUrl);

            Console.WriteLine($"🔄 Loading URL: {baseApiUrl}/{relativeUrl}");

            if (string.IsNullOrEmpty(result))
            {
                Console.WriteLine($"⚠️ Chyba při načítání varianty {variantId} – odpověď je prázdná nebo null.");
                continue;
            }

            Console.WriteLine("📦 Response Content: " + result);

            try
            {
                using var doc = JsonDocument.Parse(result);
                var root = doc.RootElement;

                string name = root.GetProperty("name").GetString() ?? "(bez názvu)";
                int? stock = root.TryGetProperty("stock_quantity", out var stockElement) && stockElement.ValueKind != JsonValueKind.Null
                    ? stockElement.GetInt32()
                    : (int?)null;

                // Atributy
                var attributes = new Dictionary<string, List<string>>();

                if (root.TryGetProperty("attributes", out var attributesElement) && attributesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var attr in attributesElement.EnumerateArray())
                    {
                        string attrName = attr.GetProperty("name").GetString() ?? "";
                        if (attr.TryGetProperty("option", out var optionElement))
                        {
                            string option = optionElement.GetString() ?? "";
                            if (!attributes.ContainsKey(attrName))
                                attributes[attrName] = new List<string>();
                            attributes[attrName].Add(option);
                        }
                    }
                }

                details.Add(new Product
                {
                    WooId = variantId,
                    Name = name,
                    Stock = stock ?? 0,
                    Attributes = attributes
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Chyba při parsování varianty {variantId}: {ex.Message}");
            }
        }

        return details;
    }
}
//    public async Task<Dictionary<int, int>> LoadVariantQuantitiesAsync(int productId)
//    {
//        IConfiguration config = new ConfigurationBuilder()
//            .AddUserSecrets<Program>()
//            .Build();

//        var variationsUrl = config["WooCommerce:ApiUrl"] + $"/products/{productId}/variations?per_page=100";

//        var variations = new Dictionary<int, int>();

//        var response = await client.GetAsync(variationsUrl);
//        if (!response.IsSuccessStatusCode)
//        {
//            Console.WriteLine($"Chyba při načítání variant pro produkt {productId}: {response.StatusCode}");
//            return variations;
//        }

//        var result = await response.Content.ReadAsStringAsync();
//        using JsonDocument doc = JsonDocument.Parse(result);

//        foreach (var variation in doc.RootElement.EnumerateArray())
//        {
//            int variationId = variation.GetProperty("id").GetInt32();
//            if (variation.TryGetProperty("attributes", out var attributesArray) && attributesArray.ValueKind == JsonValueKind.Array)
//            {
//                foreach (var attr in attributesArray.EnumerateArray())
//                {
//                    if (attr.TryGetProperty("name", out var nameElement) && nameElement.GetString()?.ToLower().Contains("balen") == true)
//                    {
//                        if (attr.TryGetProperty("option", out var optionElement) && optionElement.ValueKind == JsonValueKind.String)
//                        {
//                            string option = optionElement.GetString()!;
//                            int quantity = ParseQuantityFromOption(option);

//                            if (quantity > 0)
//                            {
//                                variations[variationId] = quantity;
//                            }
//                        }
//                    }
//                }
//            }
//        }

//        return variations;
//    }

//    private int ParseQuantityFromOption(string option)
//    {
//        // například převést "10 ks" → 10
//        var digits = new string(option.Where(char.IsDigit).ToArray());
//        return int.TryParse(digits, out var quantity) ? quantity : 1;
//    }

//}
