using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DamDucDuy.Models
{
    public class Tag
    {
        public int TagID { get; set; }

        public String TagName { get; set; }

        public ICollection <Support> Supports { get; set; }
    }
}
