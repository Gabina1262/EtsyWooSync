using EtsyWooSync.Models;

namespace EtsyWooSync.Services;

public static class StockDistributor
{
    public static List<StockUpdate> DistributeStockWithOptionalColor(
        Dictionary<string, int> stockByGroup,
        List<ProductCoinVariant> allVariants)
    {
        var updates = new List<StockUpdate>();

        var groups = allVariants
            .Select(v => string.IsNullOrWhiteSpace(v.Color) ? "__default__" : v.Color.Trim())
            .Distinct();

        foreach (var group in groups)
        {
            if (!stockByGroup.TryGetValue(group, out int wholeBunch))
            {
                Console.WriteLine($"⚠️  Chybí WholeBunch zásoba pro skupinu: {group}");
                continue;
            }

            var groupVariants = allVariants
                .Where(v =>
                    (string.IsNullOrWhiteSpace(v.Color) && group == "__default__") ||
                    (v.Color?.Trim().Equals(group, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

            Console.WriteLine($"🔄 Barva {group} – výchozí zásoba: {wholeBunch}");

            foreach (var variant in groupVariants.OrderByDescending(v => v.QuantityPerPackage))
            {
                if (variant.QuantityPerPackage <= 0)
                {
                    Console.WriteLine($"⚠️  Varianta ID {variant.VariantId} má neplatné balení: {variant.QuantityPerPackage}");
                    continue;
                }

                int count = wholeBunch / variant.QuantityPerPackage;

                updates.Add(new StockUpdate
                {
                    VariantId = variant.VariantId,
                    NewStockQuantity = count
                });

                Console.WriteLine($"📦  {variant.QuantityPerPackage} ks balení → {count} balení (ID {variant.VariantId})");
            }
        }

        return updates;
    }


}



