﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Models
{
    public class Person
    {
        public int PersonID { get; set; }

        [Required]
        public String PersonDisplayName { get; set; }

        public String PersonLastName { get; set; }
        public String PersonFirstName { get; set; }

        public String PersonNickname { get; set; }
    public String PersonLocation { get; set; }
        public String PersonAbout { get; set; }
        public String PersonEmail { get; set; }
        public DateTime PersonBirthday { get; set; }
        public String PersonCareer { get; set; }
        public bool Actived { get; set; }

        public ICollection<Answer> Answers { get; set; }
        public ICollection<Question> Questions { get; set; }
        public ICollection<Tag> Tags { get; set; }
    }
}
