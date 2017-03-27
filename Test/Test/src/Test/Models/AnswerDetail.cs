using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Models
{
    public class AnswerDetail
    {
        public int AnswerDetailID { get; set; }

        public Question Question { get; set; }

        public ICollection<Person> Person { get; set; }
    }
}
