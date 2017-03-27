using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Models
{
    public class Answer
    {
        public int AnswerID { get; set; }
        

        public String AnswerDescription { get; set; }
    
        [ForeignKey("Question")]
        public int QuestionID { get; set; }
        public Question Question { get; set; }

        public int PersonID { get; set; }
        public Person Person { get; set; }
        public ICollection<Comment> Comments { get; set; }
   
        public Vote2 Vote2 { get; set; }
    }
}
