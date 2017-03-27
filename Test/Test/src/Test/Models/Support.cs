using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Models
{
    public class Support
    {
        public int SupportID { get; set; }

        public int TagID { get; set; }
        public Tag Tag { get; set; }
        public int QuestionID { get; set; }
        public Question Question { get; set; }
    }
}
