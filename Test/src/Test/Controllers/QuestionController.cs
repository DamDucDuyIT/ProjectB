using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Test.Data;
using Test.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Test.Models.ViewModels;

namespace Test.Controllers
{

    public class QuestionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private int pageSize = 5;

        public QuestionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Question
        public async Task<IActionResult> Index(string searchText,int page = 1, string sortOrder = "latest")
        {
            ViewData["SearchText"] = searchText;
            ViewData["SortOrder"] = sortOrder;
            IQueryable<Question> questions = _context.Questions
                 .Include(s => s.Supports)
                     .ThenInclude(t => t.Tag);
                 

            if (!String.IsNullOrEmpty(searchText))
            {
                questions = questions.Where(s => s.QuestionDescription.Contains(searchText));
            }
            if (sortOrder == "latest")
            {
                questions=questions.OrderByDescending(q=>q.DateTime);
            }
            else if (sortOrder =="hot")
            {
                questions = questions.OrderByDescending(q => q.View);
            }
            else if (sortOrder == "vote")
            {
                questions = questions.OrderByDescending(q => q.QuestionVote);
            }

            IEnumerable<Question> questionsList = await questions .Skip((page - 1) * pageSize)
                      .Take(pageSize).ToListAsync();
            

            ListViewModel listViewModel = new ListViewModel
            {
                Questions = questionsList,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = await _context.Questions
                 .Include(s => s.Supports)
                     .ThenInclude(t => t.Tag).CountAsync()
                }
            };
    
             
            return View(listViewModel);

        }

        // GET: Question/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(a=>a.Answers)
                    .ThenInclude(o=>o.Person)
                .Include(p=>p.Person)
                .Include(s => s.Supports)
                    .ThenInclude(t => t.Tag)
                    .SingleOrDefaultAsync(m => m.QuestionID == id);
            question.View++;
            await _context.SaveChangesAsync();
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }



        // GET: Question/Create
        [Authorize]
        public IActionResult Ask()
        {
            return View();
        }

        // POST: Question/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask([Bind("QuestionID,QuestionDescription,QuestionTitle")] Question question,string selectedTags)
        {
            if (ModelState.IsValid)
            {
                question.Answers = new List<Answer>();
                question.Comments = new List<Comment>();
                question.Supports = new List<Support>();
                question.QuestionVote = 0;
                question.AnswerCount = 0;
                question.View = 0;
                question.DateTime = DateTime.Now;
                var user = await GetCurrentUserAsync();
                var userEmail = user.Email;
                var person = _context.Persons
                            .Include(s=>s.Support2s)
                    .FirstOrDefault(m => m.PersonEmail == userEmail);
                person.Score += 3;
                _context.Update(person);
                question.Person = person;
                _context.Add(question);
               
                List<String> selectedTagHS = Regex.Split(selectedTags, @"\s+").ToList();


                foreach (var tag in selectedTagHS)
                {
                    if (_context.Tags.Any(t => t.TagName == tag.ToLower()))
                    {
                        question.Supports.Add(new Support { TagID = _context.Tags.FirstOrDefault(t=>t.TagName==tag.ToLower()).TagID, QuestionID = question.QuestionID });
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        _context.Tags.Add(new Tag { TagName=tag.ToLower()});
                        await _context.SaveChangesAsync();
                        question.Supports.Add(new Support { TagID = _context.Tags.FirstOrDefault(t => t.TagName == tag.ToLower()).TagID, QuestionID = question.QuestionID });
                        await _context.SaveChangesAsync();
                    }
                }
                await _context.SaveChangesAsync();

                //foreach (var item in question.Supports)
                //{

                //    person.Tags.Add(item.Tag);

                //}
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return View(question);
        }

        // GET: Question/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions.SingleOrDefaultAsync(m => m.QuestionID == id);
            if (question == null)
            {
                return NotFound();
            }
            return View(question);
        }

        // POST: Question/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("QuestionID,QuestionDescription,QuestionTitle,QuestionVote")] Question question)
        {
            if (id != question.QuestionID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(question);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(question.QuestionID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(question);
        }
        [Authorize]
        public async Task<IActionResult> Vote(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions.Include(p=>p.Person).SingleOrDefaultAsync(m => m.QuestionID == id);
            int personId = question.Person.PersonID;
            question.QuestionVote++;
            _context.Update(question);

            await _context.SaveChangesAsync();
            
            var person = await _context.Persons.SingleOrDefaultAsync(p => p.PersonID == personId);
            person.Score += 5;
            _context.Update(person);

            await _context.SaveChangesAsync();
            return RedirectToAction("Details",new { id = id });
        }
        [Authorize]
        public async Task<IActionResult> TakeAnswer(int? id,string answerText)
        {
            ViewData["answerText"] = answerText;

            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                            .Include(s=>s.Supports)
                                
                .SingleOrDefaultAsync(m => m.QuestionID == id);

            var user = await GetCurrentUserAsync();
            var userEmail = user.Email;
            var person = _context.Persons
                    .Include(s=>s.Support2s)
                .FirstOrDefault(m => m.PersonEmail == userEmail);
            person.Score += 3;
            _context.Update(person);


            //foreach(var item in question.Supports)
            //{
               
            //   person.Tags.Add(_context.Tags.FirstOrDefault(t=>t.TagID== item.TagID));
                              
            //}
            await _context.SaveChangesAsync();


            Answer answer = new Answer();
            answer.AnswerDescription = answerText;
            answer.Vote = 0;
            answer.Person = person;
            answer.Question = question;
            _context.Add(answer);
            await _context.SaveChangesAsync();

            question.Answers.Add(answer);
            question.AnswerCount++;
            _context.Update(question);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = id });
        }

        // GET: Question/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions.SingleOrDefaultAsync(m => m.QuestionID == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Question/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _context.Questions.SingleOrDefaultAsync(m => m.QuestionID == id);
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.QuestionID == id);
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

    }
}
