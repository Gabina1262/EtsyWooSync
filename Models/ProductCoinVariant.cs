using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtsyWooSync.Models
{
    public class ProductCoinVariant
    {
        public int ProductId { get; set; }              // ID hlavního produktu
        public int VariantId { get; set; }              // ID varianty
        public string? Color { get; set; }              // např. "patinovana-stribrna"
        public int QuantityPerPackage { get; set; }     // 1, 10 nebo 100
    }
}
