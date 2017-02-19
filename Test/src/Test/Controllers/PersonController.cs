using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Test.Data;
using Test.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections;
using Microsoft.AspNetCore.Authorization;

namespace Test.Controllers
{
    [Authorize]
    public class PersonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public PersonController(SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;

        }

        // GET: Person
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            IQueryable<Person> persons = _context.Persons.Where(p => p.Actived == true);
            return View(await persons.ToListAsync());
        }

        // GET: Person
        public async Task<IActionResult> Profile()
        {

            var user = await GetCurrentUserAsync();
            var userEmail = user.Email;
            var person =  _context.Persons.FirstOrDefault(m => m.PersonEmail == userEmail);
            return View(person);
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        // GET: Person/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.Persons.SingleOrDefaultAsync(m => m.PersonID == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // GET: Person/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Person/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PersonID,PersonAbout,PersonBirthday,PersonCareer,PersonDisplayName,PersonEmail,PersonFirstName,PersonLastName,PersonLocation")] Person person)
        {
            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                person.PersonEmail = user.Email;
                _context.Add(person);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(person);
        }

        // GET: Person/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.Persons.SingleOrDefaultAsync(m => m.PersonID == id);
            if (person == null)
            {
                return NotFound();
            }
            return View(person);
        }

        // POST: Person/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PersonID,PersonAbout,PersonBirthday,PersonCareer,PersonDisplayName,PersonEmail,PersonFirstName,PersonLastName,PersonLocation")] Person person)
        {
            if (id != person.PersonID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(person);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonExists(person.PersonID))
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
            return View(person);
        }

        // GET: Person/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.Persons.SingleOrDefaultAsync(m => m.PersonID == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // POST: Person/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var person = await _context.Persons.SingleOrDefaultAsync(m => m.PersonID == id);
            _context.Persons.Remove(person);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool PersonExists(int id)
        {
            return _context.Persons.Any(e => e.PersonID == id);
        }
    }
}
