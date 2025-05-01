using EtsyWooSync.Models;

public static class VariantDebugHelper
{
    public static void PrintVariantIds(IEnumerable<ProductCoinVariant> variants)
    {
        foreach (var variant in variants)
        {
            Console.WriteLine($"Variant ID: {variant.VariantId}");
        }
    }
}
