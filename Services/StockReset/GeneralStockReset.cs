using EtsyWooSync.Inerface;

using EtsyWooSync.Models;
using EtsyWooSync.Services.StockReset;

namespace EtsyWooSync.Services.StockReset
{
    public class GeneralStockReset
    {
        private readonly CoinStockReset _coinStockReset;
        private readonly SetStockReset _setStockReset;

        public GeneralStockReset(CoinStockReset coinStockReset, SetStockReset setStockReset)
        {
            _coinStockReset = coinStockReset;
            _setStockReset = setStockReset;
        }

        public async Task ResetAllAsync(List<ProductSnapshot> coinSnapshot, List<IProduct> allProducts)
        {
            Console.WriteLine("=== PŘEPOČET SKLADŮ: ZAČÍNÁM ===");

            if (coinSnapshot.Count == 0)
            {
                Console.WriteLine("Žádné mince nebyly nalezeny.");
            }
            else
            {
                await _coinStockReset.ResetCoinStockAsync(coinSnapshot);
            }

            _setStockReset.ResetSetStockAsync(allProducts);

            Console.WriteLine("=== PŘEPOČET SKLADŮ: HOTOVO ===");
        }
    }
}
