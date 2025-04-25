namespace EtsyWooSync.Inerface
{
    public interface IProduct
    {
        public string Name { get;  }// název produktu
        public int Stock { get;  }  // počet kusů 

        public int WooId { get; } // ID produktu na WooCommerce   
    }
}
