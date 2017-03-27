using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Models
{
    public class Question
    {
        public int QuestionID { get; set; }

        [DisplayName("Tiêu đề")]
        [Required(ErrorMessage = "Yêu cầu nhập Tiêu đề")]
        public string QuestionTitle { get; set; }

        [DisplayName("Nội Dung")]
        [Required(ErrorMessage = "Yêu cầu nhập Nội dung")]
        public string QuestionDescription { get; set; }

        public int QuestionVote { get; set; }

        public int AnswerCount { get; set; }

        public Person Person { get; set; }

        public DateTime DateTime { get; set; }

        public int View { get; set; }

        public ICollection<Answer> Answers { get; set; }

        public ICollection<Comment> Comments { get; set; }

        public ICollection<Support> Supports { get; set; }

        public Vote Vote { get; set; }

    }
}