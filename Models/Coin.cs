using EtsyWooSync.Inerface;

public abstract partial class Coin : IProduct, IHasCategories, IHasTags, IHasAttributes
{

    public int WooId { get; set; } = 0; // ID produktu na WooCommerce
    public string Name { get; set; } = string.Empty; // název mince

    public int Stock => WholeBunch;
    public virtual int WholeBunch { get; set; } = 0; // počet kusů v setu   

    public  Dictionary<int, int> Variations { get; set; } = new();
    public virtual List<string> Categories { get; set; } = new()
    {
        "Mince", "Novinky", "Rekvizity"
    };

    public virtual List<string> Tags { get; set; } = new()
    {
        "mince"
    };

    public abstract Dictionary<string, List<string>> Attributes { get; set; }

    bool MoreThanHundred { get; set; } = true;

    public void ProcessCoinOrder(int variationId, int quantity)
    {
        if (!Variations.ContainsKey(variationId))
        {
            Console.WriteLine($"Neznámá varianta: {variationId}");
            return;
        }

        int unitsPerItem = Variations[variationId];
        int totalUnits = unitsPerItem * quantity;

        if (totalUnits > WholeBunch)
        {
            Console.WriteLine($"Nedostatek na skladě {Name}: potřeba {totalUnits}, dostupné {WholeBunch}");
        }
        else
        {
            WholeBunch -= totalUnits;
            Console.WriteLine($"Zpracována objednávka: {quantity}x varianta {variationId} ({unitsPerItem} ks) → odečteno {totalUnits}. Zbývá: {WholeBunch}");
        }

        if (WholeBunch < 100)
        {
            Console.WriteLine($"UPOZORNĚNÍ: {Name} dochází, zbývá jen {WholeBunch} ks!");
        }
    }

    public async Task RestockCoin(int id, int wholeBunch)
    {
        await Task.Run(() =>
            {
                WholeBunch = wholeBunch;
                Console.WriteLine($"Doplněno {Name}: {WholeBunch} ks");
            });
    }
}