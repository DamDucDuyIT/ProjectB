using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Test.Data;

namespace Test.Models
{
    public class SeedData
    {
        public static void EnsurePopulated(IApplicationBuilder app)
        {
            ApplicationDbContext context = app.ApplicationServices
                .GetRequiredService<ApplicationDbContext>();

            context.Database.EnsureCreated();

            if (context.Questions.Any())
            {
                return;
            }
            Tag java = new Tag { TagName = "Java" };
            Tag cSharp = new Tag { TagName = "C#" };
            Tag cPlusPlus = new Tag { TagName = "c++" };
            Tag javaScript = new Tag { TagName = "Java Script" };
            Tag sql = new Tag { TagName = "Sql" };

            context.Tags.AddRange(java, cSharp, cPlusPlus, javaScript, sql);
            context.SaveChanges();



            Person person1 = new Person { PersonDisplayName = "Đàm Đức Duy", PersonLastName = "Đàm", PersonFirstName = "Đức Duy", PersonLocation = "Thủ Dầu Một, Bình Dương", PersonAbout = "yêu thiên nhiên, thích lập trình", PersonBirthday = DateTime.Parse("1995-10-18"), PersonEmail = "duy.dam.k3set@eiu.edu.vn", PersonCareer = "Student" };
            Person person2 = new Person { PersonDisplayName = "Trần Phúc Hậu", PersonLastName = "Trần", PersonFirstName = "Phúc Hậu", PersonLocation = "Bình Dương", PersonAbout = "yêu mèo, thích con trai", PersonBirthday = DateTime.Parse("1995-08-11"), PersonEmail = "hau.tran.k3set@eiu.edu.vn", PersonCareer = "Student" };
            Person person3 = new Person { PersonDisplayName = "Tất Quảng Phát", PersonLastName = "Tất", PersonFirstName = "Quảng Phát", PersonLocation = "Hồ Chí Minh", PersonAbout = "giáo viên EIU", PersonBirthday = DateTime.Parse("1969-09-02"), PersonEmail = "phat.tat@eiu.edu.vn", PersonCareer = "Lecturer" };

            context.Persons.AddRange(person1, person2, person3);
            context.SaveChanges();


            Question question1 = new Question { QuestionTitle = "HashMap trong c#", QuestionDescription = "cach sử dụng HashMap trong c#?", QuestionVote = 0 };
            Question question2 = new Question { QuestionTitle = "Sort trong Java", QuestionDescription = "làm thế nào để sort trongjava", QuestionVote = 0, Person = person2 };
            Question question3 = new Question { QuestionTitle = "phiên bản mới nhất của .Net", QuestionDescription = "Cho mình hỏi phiên bản .net mới nhất là bao nhiêu?", QuestionVote = 0, Person = person3 };

            context.Questions.AddRange(question1, question2, question3);
            context.SaveChanges();

            context.Comments.AddRange(
                new Comment { CommentDescripstion = "câu hỏi hay quá", Person = person1 },
                new Comment { CommentDescripstion = "dễ vậy mà không biết", Person = person3 }
                );
            context.SaveChanges();

            context.Answers.AddRange(
                new Answer { AnswerDescription = "dùng Dictionary", Person = person2, Question = question1, Vote = 0 },
                new Answer { AnswerDescription = "dùng Array.sort()", Person = person3, Question = question2, Vote = 0 }
                );
            context.SaveChanges();


            Support sp1 = new Support { TagID = cSharp.TagID, QuestionID = question1.QuestionID };
            Support sp2 = new Support { TagID = java.TagID, QuestionID = question2.QuestionID };
            
            context.Supports.AddRange(sp1,sp2);
            context.SaveChanges();
        }
    }
}
