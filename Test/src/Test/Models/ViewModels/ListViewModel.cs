using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Models.ViewModels
{
    public class ListViewModel
    {
        public IEnumerable<Question> Questions { get; set; }
        public IEnumerable<Person> Persons { get; set; }
        public IEnumerable<Tag> Tags { get; set; }

        public PagingInfo PagingInfo { get; set; }
    }
}
