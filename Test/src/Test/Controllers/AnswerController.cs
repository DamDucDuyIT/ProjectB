using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Test.Data;
using Test.Models;

namespace Test.Controllers
{
    public class AnswerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnswerController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Answer
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Answers.Include(a => a.Person);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Answer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var answer = await _context.Answers.SingleOrDefaultAsync(m => m.AnswerID == id);
            if (answer == null)
            {
                return NotFound();
            }

            return View(answer);
        }

        // GET: Answer/Create
        public IActionResult Create()
        {
            ViewData["PersonID"] = new SelectList(_context.Persons, "PersonID", "PersonDisplayName");
            return View();
        }

        // POST: Answer/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AnswerID,AnswerDescription,PersonID,Vote")] Answer answer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(answer);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["PersonID"] = new SelectList(_context.Persons, "PersonID", "PersonDisplayName", answer.PersonID);
            return View(answer);
        }

        // GET: Answer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var answer = await _context.Answers.SingleOrDefaultAsync(m => m.AnswerID == id);
            if (answer == null)
            {
                return NotFound();
            }
            ViewData["PersonID"] = new SelectList(_context.Persons, "PersonID", "PersonDisplayName", answer.PersonID);
            return View(answer);
        }

        // POST: Answer/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AnswerID,AnswerDescription,PersonID,Vote")] Answer answer)
        {
            if (id != answer.AnswerID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(answer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnswerExists(answer.AnswerID))
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
            ViewData["PersonID"] = new SelectList(_context.Persons, "PersonID", "PersonDisplayName", answer.PersonID);
            return View(answer);
        }
        public async Task<IActionResult> Vote(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var answer = await _context.Answers.Include(q => q.Question).SingleOrDefaultAsync(m => m.AnswerID == id);
            answer.Vote++;
            _context.Update(answer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details","Question", new { id = answer.Question.QuestionID });
        }

        // GET: Answer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var answer = await _context.Answers.SingleOrDefaultAsync(m => m.AnswerID == id);
            if (answer == null)
            {
                return NotFound();
            }

            return View(answer);
        }

        // POST: Answer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var answer = await _context.Answers.SingleOrDefaultAsync(m => m.AnswerID == id);
            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool AnswerExists(int id)
        {
            return _context.Answers.Any(e => e.AnswerID == id);
        }
    }
}
