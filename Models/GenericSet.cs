using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtsyWooSync.Models
{
    class GenericSet : ProductSet
    {
        public  List<string> Tags { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public  Dictionary<string, List<string>> Attributes { get; set; } = new();
    }
}
