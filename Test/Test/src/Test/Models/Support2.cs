using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Models
{
    public class Support2
    {
        public int Support2ID { get; set; }

        public int TagID { get; set; }
        public Tag Tag { get; set; }
        public int PersonID { get; set; }
        public Question Person { get; set; }
    }
}
