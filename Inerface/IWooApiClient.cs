using EtsyWooSync.Models;
using System.Text.Json;

namespace EtsyWooSync.Inerface
{
    public interface IWooApiClient
    {

        Task<string> GetAsync(string relativeUrl);
        Task<List<JsonElement>> GetOrdersAsync();
        Task<List<JsonElement>> GetTodaysOrdersAsync();

        Task<List<ProductVariant>> GetVariantsForProductAsync(int productId);

        Task<bool> UpdateProductStockAsync(int productId, int? stock);
        Task<bool> UpdateVariantStockAsync(int productId, int variantId, int newStockQuantity);

        Task<bool> DecreaseVariantStockAsync(int productId, int variantId, int quantityToDeduct);
    }
}

