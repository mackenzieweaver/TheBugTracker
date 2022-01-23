using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;

namespace TheBugTracker.Controllers
{
    [Authorize]
    public class ProjectPrioritiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectPrioritiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.ProjectPriorities.ToListAsync());
        }        

        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] ProjectPriority projectPriority)
        {
            if (ModelState.IsValid)
            {
                _context.Add(projectPriority);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(projectPriority);
        }
        
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectPriority = await _context.ProjectPriorities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (projectPriority == null)
            {
                return NotFound();
            }

            return View(projectPriority);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var projectPriority = await _context.ProjectPriorities.FindAsync(id);
            _context.ProjectPriorities.Remove(projectPriority);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectPriorityExists(int id)
        {
            return _context.ProjectPriorities.Any(e => e.Id == id);
        }
    }
}
