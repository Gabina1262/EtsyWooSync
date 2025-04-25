abstract partial class Coin
{
    public class GenericCoin : Coin
    {
     
        public override List<string> Tags { get; set; } = new();
        public override List<string> Categories { get; set; } = new();
        public override Dictionary<string, List<string>> Attributes { get; set; } = new();
    }
}
      
    
