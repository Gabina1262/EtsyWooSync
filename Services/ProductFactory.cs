using EtsyWooSync.Inerface;
using EtsyWooSync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Coin;

namespace EtsyWooSync.Services
{
    class ProductFactory
    {
        public static IProduct Create(JsonElement product)
        {
            int wooId = product.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.Number
                ? idElement.GetInt32()
                : -1;

            string name = product.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String
                ? nameElement.GetString()!
                : "(bez názvu)";

            int stock = product.TryGetProperty("stock_quantity", out var stockElement) && stockElement.ValueKind == JsonValueKind.Number
                ? stockElement.GetInt32()
                : 0;

            var categories = ParseCategories(product);
            var tags = ParseTags(product);
            var attributes = ParseAttributes(product);

            bool isCoin = IsCoin(product, attributes);

            if (isCoin)
            {
                var variations = ParseVariations(product);
                return new GenericCoin
                {
                    WooId = wooId,
                    Name = name,
                    WholeBunch = stock,
                    Categories = categories,
                    Tags = tags,
                    Attributes = attributes,
                    Variations = variations
                };
            }
                if (IsProductSet(product))
                {
                    return new GenericSet
                    {
                        WooId = wooId,
                        Name = name,
                        Stock = stock,
                        Categories = categories,
                        Tags = tags,
                        Attributes = attributes
                    };
                }
            

            return new Product
            {
                WooId = wooId,
                Name = name,
                Stock = stock,
                Categories = categories,
                Tags = tags,
                Attributes = attributes
            };
        }

        private static List<string> ParseCategories(JsonElement product)
        {
            var categories = new List<string>();
            if (product.TryGetProperty("categories", out var array) && array.ValueKind == JsonValueKind.Array)
            {
                foreach (var cat in array.EnumerateArray())
                {
                    if (cat.TryGetProperty("name", out var nameElement))
                        categories.Add(nameElement.GetString()!);
                }
            }
            return categories;
        }

        private static List<string> ParseTags(JsonElement product)
        {
            var tags = new List<string>();
            if (product.TryGetProperty("tags", out var array) && array.ValueKind == JsonValueKind.Array)
            {
                foreach (var tag in array.EnumerateArray())
                {
                    if (tag.TryGetProperty("name", out var nameElement))
                        tags.Add(nameElement.GetString()!);
                }
            }
            return tags;
        }

        private static Dictionary<string, List<string>> ParseAttributes(JsonElement product)
        {
            var attributes = new Dictionary<string, List<string>>();
            if (product.TryGetProperty("attributes", out var array) && array.ValueKind == JsonValueKind.Array)
            {
                foreach (var attr in array.EnumerateArray())
                {
                    if (attr.TryGetProperty("name", out var nameElement) &&
                        attr.TryGetProperty("options", out var optionsElement) &&
                        optionsElement.ValueKind == JsonValueKind.Array)
                    {
                        var values = optionsElement.EnumerateArray()
                            .Where(o => o.ValueKind == JsonValueKind.String)
                            .Select(o => o.GetString()!)
                            .ToList();

                        attributes[nameElement.GetString()!] = values;
                    }
                }
            }
            return attributes;
        }

        private static Dictionary<int, int> ParseVariations(JsonElement product)
        {
            var variations = new Dictionary<int, int>();
            if (product.TryGetProperty("variations", out var array) && array.ValueKind == JsonValueKind.Array)
            {
                // Dummy přiřazení pro testování – reálné hodnoty se budou načítat později
                var ids = array.EnumerateArray().Select(v => v.GetInt32()).ToList();
                foreach (var id in ids)
                {
                    // Zatím dáváme vše jako "1 kus", později načteme správně z variant
                    variations[id] = 1;
                }
            }
            return variations;
        }

        private static bool IsCoin(JsonElement product, Dictionary<string, List<string>> attributes)
        {
            if (product.TryGetProperty("type", out var typeElement) &&
                typeElement.ValueKind == JsonValueKind.String &&
                typeElement.GetString() == "variable")
            {
                return attributes.Keys.Any(k => k.ToLower().Contains("balen"));
            }
            return false;
        }
        
        private static bool IsProductSet(JsonElement product)
        {
            if (product.TryGetProperty("categories", out var array) && array.ValueKind == JsonValueKind.Array)
            {
                foreach (var cat in array.EnumerateArray())
                {
                    if (cat.TryGetProperty("name", out var nameElement))
                    {
                        var name = nameElement.GetString()?.ToLower();
                        if (name != null && name.Contains("set") || name.Contains("sada") || name.Contains("balíček"))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    
    }

}