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
using Test.Models.ViewModels;
using System.Text.RegularExpressions;

namespace Test.Controllers
{
    [Authorize]
    public class PersonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private int pageSize = 15;

        public PersonController(SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;

        }

        // GET: Person
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchText,int page = 1)
        {
            IQueryable<Person> persons = _context.Persons
                    .Include(s=>s.Support2s)
                        .ThenInclude(t=>t.Tag)
                    .Include(q=>q.Questions)
                    .Include(a=>a.Answers)
                .Where(p => p.Actived == true);


            if (!String.IsNullOrEmpty(searchText))
            {
                persons = persons.Where(s => s.PersonDisplayName.Contains(searchText)
                || s.PersonFirstName.Contains(searchText) || s.PersonLastName.Contains(searchText));
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
                    TotalItems = await _context.Persons.Where(p => p.Actived == true).CountAsync()
                }
            };

            return View(listViewModel);
        }

        private object Skip(object p)
        {
            throw new NotImplementedException();
        }

        // GET: Person
        [AllowAnonymous]
        public async Task<IActionResult> Profile(int? PersonID)
        {

            var user = await GetCurrentUserAsync();
            String userEmail = null;
            if (user != null)
            {
                 userEmail = user.Email;
            }

            
            //var user = await GetCurrentUserAsync();
            //var userEmail = user.Email;
            var person =  _context.Persons.FirstOrDefault(m => m.PersonID == PersonID);
            if (userEmail!=null && person.PersonEmail.Equals(userEmail))
            {
                ViewData["UserEmailConfirm"] = "yes";
            }
            else
            {
                ViewData["UserEmailConfirm"] = "no";
            }
            
            return View(person);
        }

        public async Task<IActionResult> YourProfile()
        {

       


            var user = await GetCurrentUserAsync();
            var userEmail = user.Email;
            var person = _context.Persons.FirstOrDefault(m => m.PersonEmail==userEmail);
         

            return View(person);
        }

        public async Task<IActionResult> Activity()
        {

            var user = await GetCurrentUserAsync();
            var userEmail = user.Email;
            var person = _context.Persons
                            .Include(a=>a.Answers)
                            .Include(q=>q.Questions)
                            .Include(s=>s.Support2s)  
                                .ThenInclude(t=>t.Tag)
                        .FirstOrDefault(m => m.PersonEmail == userEmail);
            
            return View(person);
        }

        public async Task<IActionResult> QuestionTab(int? personID,int? tagID, int page = 1 )
        {
            var person = _context.Persons
                           .Include(a => a.Answers)
                           .Include(q => q.Questions)
                            .ThenInclude(s => s.Supports)
                                    .ThenInclude(t => t.Tag)
                       .FirstOrDefault(m => m.PersonID == personID);
            IEnumerable<Question> questions =  person.Questions.Skip((page - 1) * pageSize)
                      .Take(pageSize).ToList();
          

            ListViewModel listViewModel = new ListViewModel
            {
                Questions = questions,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = person.Questions.ToList().Count
                }
            };
            return View(listViewModel);
    
        }

        //public async Task<IActionResult> TagTab(int? personID, int page = 1)
        //{
        //    int pageTagsize = 15;
        //    var person = _context.Persons
        //                   .Include(a => a.Answers)
        //                   .Include(s=>s.Support2s)
        //                        .ThenInclude(t=>t.tag)
        //                   .Include(q => q.Questions)
        //                    .ThenInclude(s => s.Supports)
        //                            .ThenInclude(t => t.Tag)
        //               .FirstOrDefault(m => m.PersonID == personID);
        //    IEnumerable<Tag> tags = person.Tags.Skip((page - 1) * pageTagsize)
        //              .Take(pageSize).ToList();
        //    ListViewModel listViewModel = new ListViewModel
        //    {
        //        Tags = tags,
        //        PagingInfo = new PagingInfo
        //        {
        //            CurrentPage = page,
        //            ItemsPerPage = pageSize,
        //            TotalItems = person.Tags.ToList().Count
        //        }
        //    };
        //    return View(listViewModel);

        //}







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
        public async Task<IActionResult> Create([Bind("PersonID,PersonAbout,PersonBirthday,PersonCareer,PersonDisplayName,PersonEmail,PersonFirstName,PersonLastName,PersonLocation")] Person person, string selectedTags)
        {
            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                person.PersonEmail = user.Email;
                person.Support2s = new List<Support2>();
             
                person.Score = 0;
                person.Actived = true;
                

                List<String> selectedTagHS = Regex.Split(selectedTags, @"\W+").ToList();


                foreach (var tag in selectedTagHS)
                {
                    if (_context.Tags.Any(t => t.TagName == tag))
                    {
                        person.Support2s.Add(new Support2 { TagID = _context.Tags.FirstOrDefault(t => t.TagName == tag).TagID, PersonID = person.PersonID});
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        _context.Tags.Add(new Tag { TagName = tag });
                        await _context.SaveChangesAsync();
                        person.Support2s.Add(new Support2 { TagID = _context.Tags.FirstOrDefault(t => t.TagName == tag).TagID, PersonID = person.PersonID });
                        await _context.SaveChangesAsync();

                    }
                }

                _context.Persons.Add(person);
                
                await _context.SaveChangesAsync();

                return RedirectToAction("Index","Home");
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
