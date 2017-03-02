using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Test.Data;
using Test.Models;
using Test.Models.ViewModels;

namespace Test.Controllers
{
    public class TagController : Controller
    {
        private readonly ApplicationDbContext _context;
        private int pageSize = 15;

        public TagController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Tag
        public async Task<IActionResult> Index(String searchText,String tagName, int page =1)
        {
            ViewData["searchText"] = searchText;
            IQueryable<Tag> qryTags = _context.Tags.Include(s=>s.Supports);


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
                    TotalItems = await _context.Tags.CountAsync()
                }
            };

            return View(listViewModel);
        }

        public async Task<IActionResult> Tagged(String tagName)
        {
            IQueryable<Support> qryTags = _context.Supports.Include(t => t.Tag)

                .Include(q => q.Question).ThenInclude(s => s.Supports).ThenInclude(a => a.Tag);

            qryTags = qryTags.Where(t => t.Tag.TagName == tagName);


            return View(await qryTags.ToListAsync());
        }

        // GET: Tag/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags.SingleOrDefaultAsync(m => m.TagID == id);
            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        // GET: Tag/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tag/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TagID,TagName")] Tag tag)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tag);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(tag);
        }

        // GET: Tag/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags.SingleOrDefaultAsync(m => m.TagID == id);
            if (tag == null)
            {
                return NotFound();
            }
            return View(tag);
        }

        // POST: Tag/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TagID,TagName")] Tag tag)
        {
            if (id != tag.TagID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tag);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagExists(tag.TagID))
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
            return View(tag);
        }

        // GET: Tag/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags.SingleOrDefaultAsync(m => m.TagID == id);
            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        // POST: Tag/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tag = await _context.Tags.SingleOrDefaultAsync(m => m.TagID == id);
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool TagExists(int id)
        {
            return _context.Tags.Any(e => e.TagID == id);
        }
    }
}
