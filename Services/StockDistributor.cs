using EtsyWooSync.Models;

namespace EtsyWooSync.Services
{

    public static class StockDistributor
    {
        public static List<StockUpdate> DistributeStockWithOptionalColor(
            Dictionary<string, int> stockByGroup,
            List<ProductVariant> allVariants)
        {
            var updates = new List<StockUpdate>();

            var groups = allVariants
                .Select(v => string.IsNullOrWhiteSpace(v.Color) ? "__default__" : v.Color.Trim())
                .Distinct();

            foreach (var group in groups)
            {
                if (!stockByGroup.TryGetValue(group, out int wholeBunch))
                    continue;

                var groupVariants = allVariants
                    .Where(v =>
                        (string.IsNullOrWhiteSpace(v.Color) && group == "__default__") ||
                        (v.Color?.Trim().Equals(group, StringComparison.OrdinalIgnoreCase) ?? false))
                    .OrderByDescending(v => v.QuantityPerPackage)
                    .ToList();

                int remaining = wholeBunch;

                foreach (var variant in groupVariants)
                {
                    int count = remaining / variant.QuantityPerPackage;
                    remaining %= variant.QuantityPerPackage;

                    updates.Add(new StockUpdate
                    {
                        VariantId = variant.VariantId,
                        NewStockQuantity = count
                    });
                }
            }

            return updates;
        }
    }
}

