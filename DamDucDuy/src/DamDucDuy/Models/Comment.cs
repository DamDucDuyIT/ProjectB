using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DamDucDuy.Models
{
    public class Comment
    {
        public int CommentID { get; set; }

        public String CommentDescripstion { get; set; }

        public int PersonID { get; set; }
        public Person Person { get; set; }
        public Question Question { get; set; }
        public Answer Answer { get; set; }

    }
}
