namespace EtsyWooSync.SyncModels
{
    public class SyncCoinsMap
    {
        public int WooVariationId { get; set; }
        public string EtsyListingId { get; set; } = string.Empty; // ID produktu na Etsy
        public int UnitSize { get; set; }              // kolik fyzických kusů představuje tahle varianta
        public string ProductType { get; set; }   = string.Empty;    // název zboží (např. "Prazsky Gros")
        public float Price { get; set; }       // cena za kus
    }
}
