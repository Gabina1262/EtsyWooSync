using static Coin;

namespace EtsyWooSync.Services.Helpers
{
    class CoinHelper

    {
        public static Dictionary<int, int> CalculateVariationStocks(GenericCoin coin)
        {
            var result = new Dictionary<int, int>();

            foreach (var variationId in coin.Variations) 
            {
                int unitsPerVariation = 1; // dummy hodnota, později opravíme

                int stockForVariation = coin.WholeBunch / unitsPerVariation;
                result[variationId] = stockForVariation; // tady už je variationId správně!
            }

            return result;
        }
    }
}
