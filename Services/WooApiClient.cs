using EtsyWooSync.Inerface;
using EtsyWooSync.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;

public class WooApiClient : IWooApiClient
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
            MaxResponseHeadersLength = 128
        };

        client = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseApiUrl)
        };

        var byteArray = Encoding.ASCII.GetBytes($"{consumerKey}:{consumerSecret}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
    }

    #region Update Methods

    public async Task<bool> UpdateProductStockAsync(int id, int? stock, bool manageStock = true)
    {
        var updateUrl = $"{baseApiUrl}/products/{id}";

        Console.WriteLine($"{id} → aktualizace skladu na {stock} ks (manage_stock: {manageStock})");

        var productData = new Dictionary<string, object>();
        if (stock.HasValue)
            productData["stock_quantity"] = stock.Value;

        productData["manage_stock"] = manageStock;

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

    public async Task<bool> UpdateVariantStockAsync(int productId, int variantId, int newStockQuantity)
    {
        var updateUrl = $"{baseApiUrl}/products/{productId}/variations/{variantId}";

        Console.WriteLine($"Aktualizuji sklad → produkt {productId}, varianta {variantId} → {newStockQuantity} ks");

        var data = new Dictionary<string, object>
        {
            { "stock_quantity", newStockQuantity },
            { "manage_stock", true }
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(data),
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
            Console.WriteLine($"Chyba při aktualizaci varianty {variantId}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SmartUpdateStockAsync(IProduct product, int? stock)
    {
        if (product == null)
            return false;

        switch (product)
        {
            case ProductCoinVariant variant:
                return await UpdateVariantStockAsync(variant.ProductId, variant.VariantId, stock ?? 0);

            case Coin coin when coin.Variations?.Count > 0:
                return await UpdateProductStockAsync(product.WooId, null, manageStock: false);

            default:
                return await UpdateProductStockAsync(product.WooId, stock, manageStock: true);
        }
    }

    public async Task<bool> DecreaseVariantStockAsync(int productId, int variantId, int quantityToDeduct)
    {
        var relativeUrl = $"/products/{productId}/variations/{variantId}";
        var json = await GetAsync(relativeUrl);

        if (string.IsNullOrWhiteSpace(json))
        {
            Console.WriteLine($"Nepodařilo se načíst variantu {variantId} pro odečet.");
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            int currentStock = root.TryGetProperty("stock_quantity", out var stockProp) && stockProp.ValueKind == JsonValueKind.Number
                ? stockProp.GetInt32()
                : 0;

            int newStock = Math.Max(0, currentStock - quantityToDeduct);

            Console.WriteLine($"Odečítám {quantityToDeduct} ks → ze {currentStock} → zůstává {newStock}");

            return await UpdateVariantStockAsync(productId, variantId, newStock);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při zpracování odečtu varianty {variantId}: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Read Methods

    public async Task<string> GetAsync(string relativeUrl)
    {
        var fullUrl = $"{baseApiUrl.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
        try
        {
            var response = await client.GetAsync(fullUrl);
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při HTTP GET: {ex.Message}");
            return null;
        }
    }

    public async Task<List<IProduct>> GetAllProductsAsync()
    {
        var allProducts = new List<IProduct>();
        int page = 1;

        while (true)
        {
            var result = await GetAsync($"/products?per_page=100&page={page}");
            if (string.IsNullOrWhiteSpace(result))
                break;

            using var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;
            if (root.GetArrayLength() == 0)
                break;

            foreach (var product in root.EnumerateArray())
            {
                int id = product.GetProperty("id").GetInt32();
                string name = product.GetProperty("name").GetString();
                int? stock = product.TryGetProperty("stock_quantity", out var stockProp) && stockProp.ValueKind != JsonValueKind.Null
                    ? stockProp.GetInt32()
                    : (int?)null;

                allProducts.Add(new Product
                {
                    WooId = id,
                    Name = name,
                    Stock = stock ?? 0
                });
            }

            page++;
        }

        return allProducts;
    }

    public async Task<List<ProductCoinVariant>> GetVariantsForCoinsAsync(int productId)
    {
        var response = await GetAsync($"/products/{productId}/variations?per_page=100");
        var variants = new List<ProductCoinVariant>();

        if (string.IsNullOrEmpty(response))
            return variants;

        using var doc = JsonDocument.Parse(response);
        foreach (var variant in doc.RootElement.EnumerateArray())
        {
            int variantId = variant.GetProperty("id").GetInt32();
            variants.Add(new ProductCoinVariant { ProductId = productId, VariantId = variantId });
        }

        return variants;
    }
    public async Task<List<int>> LoadVariantIdsAsync(int productId)
    {
        var variationsId = new List<int>();
        var relativeUrl = $"products/{productId}/variations?per_page=100";

        var result = await GetAsync(relativeUrl);

        if (string.IsNullOrEmpty(result))
        {
            Console.WriteLine($"Varianta nebyla načtena (prázdná odpověď) pro produkt {productId}");
            return variationsId;
        }

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
            Console.WriteLine($"Chyba při parsování variant produktu {productId}: {ex.Message}");
        }

        return variationsId;
    }

    #endregion
}
