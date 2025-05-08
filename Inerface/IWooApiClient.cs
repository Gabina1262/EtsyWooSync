using EtsyWooSync.Models;
using System.Text.Json;

namespace EtsyWooSync.Inerface
{
    public interface IWooApiClient
    {
        Task<string> GetAsync(string relativeUrl);

        Task<List<IProduct>> GetAllProductsAsync();

        Task<List<ProductCoinVariant>> GetVariantsForCoinsAsync(int productId);

        Task<bool> UpdateProductStockAsync(int id, int? stock, bool manageStock = true);

        Task<bool> UpdateVariantStockAsync(int productId, int variantId, int newStockQuantity);

        Task<bool> SmartUpdateStockAsync(IProduct product, int? stock);

        Task<bool> DecreaseVariantStockAsync(int productId, int variantId, int quantityToDeduct);
        Task<List<int>> LoadVariantIdsAsync(int productId);
    }
}


