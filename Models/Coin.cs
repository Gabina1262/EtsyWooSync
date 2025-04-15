abstract class Coin
{
    string Name { get; set; }
    int WholeBunch { get; set; }    
    Dictionary<int, int> Variations { get; set; } = new Dictionary<int, int>();

    bool MoreThanHundred { get; set; } = true;

    public void ProcessOrder(int variationId, int quantity)
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
}