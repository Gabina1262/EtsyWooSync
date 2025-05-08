using EtsyWooSync.Inerface;
using EtsyWooSync.Models;

namespace EtsyWooSync.Services.StockReset;

public class SetStockReset
{

    private readonly ProductSetService _setService = new();

    public void ResetSetStockAsync(List<IProduct> allProducts)
    {
        foreach (var set in allProducts.OfType<ProductSet>())
        {
            int available = _setService.GetAvailableStock(set, allProducts);
            set.Stock = available;

            Console.WriteLine($"Set '{set.Name}' přepočítán: skladem {set.Stock} ks.");
        }
    }
}
