using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Models
{
    public class Vote
    {
        public int VoteID { get; set; }
        public Question Question { get; set; }

        
        public ICollection <Person> Person { get; set; }
    }
}
