using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Test.Data;
using Test.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Test.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Test.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private int pageSize =9;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin
        public async Task<ActionResult> Question(string searchText, int page = 1, string sortOrder = "latest")
        {

            ViewData["SearchText"] = searchText;
            ViewData["SortOrder"] = sortOrder;
            IQueryable<Question> questions = _context.Questions
                 .Include(s => s.Supports)
                     .ThenInclude(t => t.Tag);


            if (!String.IsNullOrEmpty(searchText))
            {
                questions = questions.Where(s => s.QuestionDescription.Contains(searchText)
                || s.QuestionTitle.Contains(searchText));
            }
            if (sortOrder == "latest")
            {
                questions = questions.OrderByDescending(q => q.DateTime);
            }
            else if (sortOrder == "hot")
            {
                questions = questions.OrderByDescending(q => q.View);
            }
            else if (sortOrder == "vote")
            {
                questions = questions.OrderByDescending(q => q.QuestionVote);
            }

            IEnumerable<Question> questionsList = await questions.Skip((page - 1) * pageSize)
                      .Take(pageSize).ToListAsync();


            ListViewModel listViewModel = new ListViewModel
            {
                Questions = questionsList,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = await questions.CountAsync()
                }
            };


            return View(listViewModel);
        }

        public async Task<ActionResult> User(string searchText, int page = 1, string sortOrder = "score", string sortTag = "")
        {
            IQueryable<Person> persons = _context.Persons
                   .Include(s => s.Support2s)
                       .ThenInclude(t => t.Tag)
                   .Include(q => q.Questions)
                    .ThenInclude(v => v.Vote)
                           .ThenInclude(p => p.Person)
                   .Include(a => a.Answers)
                   .ThenInclude(v => v.Vote2)
                           .ThenInclude(p => p.Person)
               .Where(p => p.Actived == true);

            if (sortTag != "" && sortTag != null)
            {
                persons = persons.Where(p => p.Support2s.Any(s => s.Tag.TagName == sortTag));
            }


            if (!String.IsNullOrEmpty(searchText))
            {
                persons = persons.Where(s => s.PersonDisplayName.Contains(searchText)
                || s.PersonFirstName.Contains(searchText) || s.PersonLastName.Contains(searchText));
            }

            foreach (var person in persons)
            {
                int score = person.Questions.Count * 3 + person.Answers.Count * 3;
                foreach (var question in person.Questions)
                {
                    if (question.Vote.Person == null)
                    {
                        continue;
                    }
                    score += question.Vote.Person.Count * 5;
                }
                foreach (var answer in person.Answers)
                {
                    if (answer.Vote2.Person == null)
                    {
                        continue;
                    }
                    score += answer.Vote2.Person.Count * 10;
                }

                person.Score = score;

            }

            await _context.SaveChangesAsync();
            if (sortOrder == "score")
            {
                persons = persons.OrderByDescending(p => p.Score);
            }


            IEnumerable<Person> personsList = await persons.Skip((page - 1) * pageSize)
                      .Take(pageSize).ToListAsync();
            ListViewModel listViewModel = new ListViewModel
            {
                Persons = personsList,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = await persons.CountAsync()
                }
            };

            return View(listViewModel);

        }

        public async Task<IActionResult> Tag(String searchText, String tagName, int page = 1)
        {
            ViewData["searchText"] = searchText;
            IQueryable<Tag> qryTags = _context.Tags.Include(s => s.Supports);


            if (!String.IsNullOrEmpty(searchText))
            {
                qryTags = qryTags.Where(s => s.TagName.Contains(searchText));
            }


            IEnumerable<Tag> tagList = await qryTags.Skip((page - 1) * pageSize)
                      .Take(pageSize).ToListAsync();

            ListViewModel listViewModel = new ListViewModel
            {
                Tags = tagList,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = await qryTags.CountAsync()
                }
            };

            return View(listViewModel);
        }

        // GET: Admin/Details/5
        public async Task<ActionResult> Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var question = await _context.Questions
        .Include(a => a.Answers)
            .ThenInclude(o => o.Person)
        .Include(a => a.Answers)
            .ThenInclude(v => v.Vote2)
                .ThenInclude(p => p.Person)
        .Include(v => v.Vote)
            .ThenInclude(p => p.Person)
        .Include(p => p.Person)
        .Include(s => s.Supports)
            .ThenInclude(t => t.Tag)
            .SingleOrDefaultAsync(m => m.QuestionID == id);
            
            await _context.SaveChangesAsync();
            return View(question);
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Admin/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Admin/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Admin/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int QuestionID, IFormCollection collection)
        {
            await deleteQuestion(QuestionID);
            return RedirectToAction("Question");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAnswer(int AnswerID,int QuestionID)
        {
            await deleteAnswer(AnswerID);
            return RedirectToAction("Details", new { id = QuestionID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteTag(int TagID)
        {
            var tag = await _context.Tags.SingleOrDefaultAsync(m => m.TagID == TagID);
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return RedirectToAction("Tag");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteUser(int PersonID)
        {
            var person = await _context.Persons
                .Include(q=>q.Questions)
                .Include(s=>s.Support2s)               
                .Include(a=>a.Answers)
                .SingleOrDefaultAsync(m => m.PersonID == PersonID);
            
          
            if (person.Questions != null)
            {
                List<Question> questions =  person.Questions.ToList();
                foreach ( var question in questions)
                {
                    await deleteQuestion(question.QuestionID);
                }
            }
            if (person.Answers != null)
            {
                List<Answer> answers = person.Answers.ToList();
                foreach (var answer in answers)
                {
                    await deleteAnswer(answer.AnswerID);
                }
            }
            if (person.Support2s != null)
            {
                List<Support2> support2s = person.Support2s.ToList();
                foreach (var support2 in support2s)
                {
                    _context.Support2s.Remove(support2);
                }
            }
            var user = _context.Users.FirstOrDefault(u => u.Email == person.PersonEmail);
            _context.Users.Remove(user);

            _context.Persons.Remove(person);
            await _context.SaveChangesAsync();


            return RedirectToAction("User");
        }

        public async 
        Task
deleteQuestion(int QuestionID)
        {
            var question = await _context.Questions
                .Include(a=>a.Answers)
                    .ThenInclude(v=>v.Vote2)
                .Include(s=>s.Supports)
                .SingleOrDefaultAsync(m => m.QuestionID == QuestionID);
            if (question.Answers != null)
            {
                List<Answer> answers = question.Answers.ToList();
                foreach (var answer in answers)
                {
                    await deleteAnswer(answer.AnswerID);
                }
            }
            if (question.Supports != null)
            {
                List<Support> supports = question.Supports.ToList();
                foreach (var support in supports)
                {
                    _context.Supports.Remove(support);
                }
            }
            if (question.Vote != null)
            {              
                _context.Votes.Remove(question.Vote);
                
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
        }
        public async         Task
deleteAnswer(int AnswerID)
        {
            var answer = await _context.Answers
                .Include(v=>v.Vote2)
                    .ThenInclude(p=>p.Person)
                .SingleOrDefaultAsync(m => m.AnswerID == AnswerID);
            if (answer.Vote2 != null)
            {
                if(answer.Vote2.Person != null)
                {
                    var persons = answer.Vote2.Person.ToList();
                    foreach (var person in persons)
                    {
                        answer.Vote2.Person.Remove(person);
                    }
                }
                await _context.SaveChangesAsync();
                _context.Vote2s.Remove(answer.Vote2);
                await _context.SaveChangesAsync();

            }
            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();
        }
    }
}