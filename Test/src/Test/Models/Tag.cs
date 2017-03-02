using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Models
{
    public class Tag
    {
        public int TagID { get; set; }

        public String TagName { get; set; }

        public ICollection <Support> Supports { get; set; }
        public ICollection<Support2> Support2s { get; set; }

    }
}
