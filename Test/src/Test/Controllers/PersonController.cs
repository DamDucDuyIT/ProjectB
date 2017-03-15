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
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Test.Controllers
{
    [Authorize]
    public class PersonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private IHostingEnvironment _environment;
        private int pageSize = 15;

        public PersonController(SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHostingEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;

        }

        // GET: Person
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchText,int page = 1, string sortOrder = "score",string sortTag="")
        {
            IQueryable<Person> persons = _context.Persons
                    .Include(s=>s.Support2s)
                        .ThenInclude(t=>t.Tag)
                    .Include(q=>q.Questions)
                    .Include(a=>a.Answers)
                .Where(p => p.Actived == true);

            if (sortTag != "")
            {
                persons = persons.Where(p => p.Support2s.Any(s => s.Tag.TagName == sortTag));
            }
            

            if (!String.IsNullOrEmpty(searchText))
            {
                persons = persons.Where(s => s.PersonDisplayName.Contains(searchText)
                || s.PersonFirstName.Contains(searchText) || s.PersonLastName.Contains(searchText));
            }

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
            var person =  _context.Persons
                .Include(s=>s.Support2s)
                    .ThenInclude(t=>t.Tag)
                .FirstOrDefault(m => m.PersonID == PersonID);
            if (userEmail!=null &&  person.PersonEmail.Equals(userEmail))
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
            var person = _context.Persons.Include(s=>s.Support2s).ThenInclude(t=>t.Tag).FirstOrDefault(m => m.PersonEmail==userEmail);
         

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
        public async Task<IActionResult> Create([Bind("PersonID,PersonAbout,PersonBirthday,PersonCareer,PersonDisplayName,PersonEmail,PersonFirstName,PersonLastName,PersonLocation")] Person person, string selectedTags, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                person.PersonEmail = user.Email;
                person.Support2s = new List<Support2>();
             
                person.Score = 0;
                person.Actived = true;

  
                List<String> selectedTagHS = Regex.Split(selectedTags, @"\s+").ToList();


                foreach (var tag in selectedTagHS)
                {
                    if (_context.Tags.Any(t => t.TagName == tag.ToLower()))
                    {
                        person.Support2s.Add(new Support2 { TagID = _context.Tags.FirstOrDefault(t => t.TagName == tag.ToLower()).TagID, PersonID = person.PersonID});
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        _context.Tags.Add(new Tag { TagName = tag.ToLower() });
                        await _context.SaveChangesAsync();
                        person.Support2s.Add(new Support2 { TagID = _context.Tags.FirstOrDefault(t => t.TagName == tag.ToLower()).TagID, PersonID = person.PersonID });
                        await _context.SaveChangesAsync();

                    }
                }

                _context.Persons.Add(person);

                await _context.SaveChangesAsync();
                person.PersonAvatar = person.PersonID + ".png";
                await _context.SaveChangesAsync();

                var uploads = Path.Combine(_environment.WebRootPath, "avatars");

                using (var fileStream = new FileStream(Path.Combine(uploads, person.PersonID.ToString() + ".png"), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

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

            var person = await _context.Persons
                .Include(s=>s.Support2s)
                    .ThenInclude(t=>t.Tag)
                .SingleOrDefaultAsync(m => m.PersonID == id);
           
            if (person == null)
            {
                return NotFound();
            }
            string tamp = "";
            foreach (var item in person.Support2s)
            {
                tamp += item.Tag.TagName + " ";
            }
            ViewData["tag"] = tamp;
            return View(person);
        }

        // POST: Person/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Actived,Score,PersonEmail,PersonID,PersonAbout,PersonBirthday,PersonCareer,PersonDisplayName,PersonEmail,PersonFirstName,PersonLastName,PersonLocation,PersonAvatar")] Person person, IFormFile file, string tag="")
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
                    IQueryable<Support2> support2 = _context.Support2s
                                                    .Include(p=>p.Person)
                                                    .Include(t=>t.Tag)
                                                    .Where(s => s.PersonID == person.PersonID);
                    foreach(var item in support2)
                    {
                        _context.Remove(item);
                      
                    }
                    await _context.SaveChangesAsync();

                    List<String> selectedTagHS = Regex.Split(tag, @"\s+").ToList();
                    foreach (var item in selectedTagHS)
                    {
                        if (item == "")
                        {
                            continue;
                        }
                        if (_context.Tags.Any(t => t.TagName == item.ToLower()))
                        {
                            person.Support2s.Add(new Support2 { TagID = _context.Tags.FirstOrDefault(t => t.TagName == item.ToLower()).TagID, PersonID = person.PersonID });
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            _context.Tags.Add(new Tag { TagName = item.ToLower() });
                            await _context.SaveChangesAsync();
                            person.Support2s.Add(new Support2 { TagID = _context.Tags.FirstOrDefault(t => t.TagName == item.ToLower()).TagID, PersonID = person.PersonID });
                            await _context.SaveChangesAsync();

                        }
                    }
                    await _context.SaveChangesAsync();
                    var uploads = Path.Combine(_environment.WebRootPath, "avatars");

                    using (var fileStream = new FileStream(Path.Combine(uploads, person.PersonID.ToString() + ".png"), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

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
                return RedirectToAction("YourProfile");
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
