using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EtsyWooSync.Storage
{
    class ProductStateStore
    {
        private const string FilePath = "storage/products.json";
        private Dictionary<int, int> _productStates = new();

        public ProductStateStore()
        {
            Load();
        }

        public int? GetWholeBunch(int productId)
        {
            return _productStates.TryGetValue(productId, out var value) ? value : null;
        }

        public void SetWholeBunch(int productId, int wholeBunch)
        {
            _productStates[productId] = wholeBunch;
            Save();
        }

        private void Load()
        {
            if (!File.Exists(FilePath))
            {
                _productStates = new Dictionary<int, int>();
                return;
            }

            var json = File.ReadAllText(FilePath);
            _productStates = JsonSerializer.Deserialize<Dictionary<int, int>>(json)
                            ?? new Dictionary<int, int>();
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(_productStates, new JsonSerializerOptions { WriteIndented = true });
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, json);
        }
    }
}
