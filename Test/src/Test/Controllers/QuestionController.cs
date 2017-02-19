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

namespace Test.Controllers
{
    public class QuestionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuestionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;  
        }

        // GET: Question
        public async Task<IActionResult> Index()
        {
            return View(await _context.Questions
                .Include(s=>s.Supports)
                    .ThenInclude(t=>t.Tag)
                .ToListAsync());
            

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


                var user = await GetCurrentUserAsync();
                var userEmail = user.Email;
                var person = _context.Persons.FirstOrDefault(m => m.PersonEmail == userEmail);
                question.Person = person;
                _context.Add(question);
               
                List<String> selectedTagHS = Regex.Split(selectedTags, @"\W+").ToList();


                foreach (var tag in selectedTagHS)
                {
                    if (_context.Tags.Any(t => t.TagName == tag))
                    {
                        question.Supports.Add(new Support { TagID = _context.Tags.FirstOrDefault(t=>t.TagName==tag).TagID, QuestionID = question.QuestionID });
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        _context.Tags.Add(new Tag { TagName=tag});
                        await _context.SaveChangesAsync();
                        question.Supports.Add(new Support { TagID = _context.Tags.FirstOrDefault(t => t.TagName == tag).TagID, QuestionID = question.QuestionID });
                        await _context.SaveChangesAsync();
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(question);
        }

        // GET: Question/Edit/5
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

        public async Task<IActionResult> Vote(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions.SingleOrDefaultAsync(m => m.QuestionID == id);
            question.QuestionVote++;
            _context.Update(question);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Details",new { id = id });
        }

        public async Task<IActionResult> TakeAnswer(int? id,string answerText)
        {
            ViewData["answerText"] = answerText;
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions.SingleOrDefaultAsync(m => m.QuestionID == id);

            var user = await GetCurrentUserAsync();
            var userEmail = user.Email;
            var person = _context.Persons.FirstOrDefault(m => m.PersonEmail == userEmail);

            Answer answer = new Answer();
            answer.AnswerDescription = answerText;
            answer.Vote = 0;
            answer.Person = person;
            answer.Question = question;
            _context.Add(answer);
            await _context.SaveChangesAsync();

            question.Answers.Add(answer);
            _context.Update(question);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = id });
        }

        // GET: Question/Delete/5
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
