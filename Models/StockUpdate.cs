using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtsyWooSync.Models
{
   public class StockUpdate
    {
        public int VariantId { get; set; }
        public int NewStockQuantity { get; set; }
    }
}
