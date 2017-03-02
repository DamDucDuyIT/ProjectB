using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Models
{
    public class Question
    {
        public int QuestionID { get; set; }
        public string QuestionTitle { get; set; }
        public string QuestionDescription { get; set; }

        public int QuestionVote { get; set; }

        public int AnswerCount { get; set; }

        public Person Person { get; set; }

        public DateTime DateTime { get; set; }

        public int View { get; set; }

        public ICollection<Answer> Answers { get; set; }

        public ICollection<Comment> Comments { get; set; }

        public ICollection<Support> Supports { get; set; }
    }
}
