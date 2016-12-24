using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Models
{
    public class Answer
    {
        public int AnswerID { get; set; }
        public int Vote { get; set; }

        public String AnswerDescription { get; set; }
    
        public Question Question { get; set; }

        public int PersonID { get; set; }
        public Person Person { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}
