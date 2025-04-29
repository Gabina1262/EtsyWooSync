using EtsyWooSync.Inerface;
using EtsyWooSync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EtsyWooSync.Services
{
  public  class ProductSnapshotExporter
    {
        private const string FilePath = "storage/products_snapshot.json";

        public static async Task ExportToJsonAsync(List<ProductSnapshot> snapshots)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            var json = JsonSerializer.Serialize(snapshots, options);
            await File.WriteAllTextAsync(FilePath, json);
            Console.WriteLine($"Snapshots exported to {FilePath}");
        }

        public static async Task<List<ProductSnapshot>> GetSnapshotsAsync()
        {
            if (!File.Exists(FilePath))
            {
                Console.WriteLine("No snapshot file found.");
                return new List<ProductSnapshot>();
            }

            var json = await File.ReadAllTextAsync(FilePath);
            var snapshots = JsonSerializer.Deserialize<List<ProductSnapshot>>(json);
            return snapshots ?? new List<ProductSnapshot>();
        }

        public static List<ProductSnapshot> CreateFromProducts(List<IProduct> products)
        {
            var snapshots = new List<ProductSnapshot>();

            foreach (var product in products)
            {
                var snapshot = new ProductSnapshot
                {
                    Id = product.WooId,
                    Name = product.Name,
                    Type = product is Coin ? "variable" : "simple",
                    TotalStock = product is Coin coin ? coin.WholeBunch : product.Stock,
                    Categories = product is IHasCategories c ? c.Categories : new List<string>(),
                    Tags = product is IHasTags t ? t.Tags : new List<string>(),
                    Attributes = product is IHasAttributes a ? a.Attributes : new Dictionary<string, List<string>>()
                };

                if (product is Coin coinProduct)
                {
                    snapshot.VariationIds = coinProduct.Variations?.Keys.ToList();
                }

                snapshots.Add(snapshot);
            }

            return snapshots;
        }
    }
}
