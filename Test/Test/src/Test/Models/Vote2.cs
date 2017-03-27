using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Models
{
    public class Vote2
    {
        public int Vote2ID { get; set; }
        public int AnswerID { get; set; }
        public Answer Answer { get; set; }


        public ICollection<Person> Person { get; set; }
    }
}
