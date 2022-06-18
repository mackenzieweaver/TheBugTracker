using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Extensions;
using TheBugTracker.Models;
using TheBugTracker.Models.ViewModels;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        private readonly IBTLookupService _lookupService;

        public ProjectsController(ApplicationDbContext context, IBTRolesService rolesService, IBTLookupService lookupService)
        {
            _context = context;
            _rolesService = rolesService;
            _lookupService = lookupService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .Include(p => p.Tickets)
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .ToListAsync();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            if(user is not null) 
                projects = projects.Where(project => project.CompanyId == user.CompanyId).ToList();
            return View(projects);
        }
        
        [AllowAnonymous]
        public async Task<IActionResult> AddTicketToProject(int projectId)
        {
            var tickets = await _context.Tickets.Where(x => x.ProjectId != projectId).ToListAsync();
            var r = (new Random()).Next(0, tickets.Count);
            tickets[r].ProjectId = projectId;
            _context.Update(tickets[r]);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = projectId });
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.Tickets).ThenInclude(x => x.TicketType)
                .Include(p => p.Tickets).ThenInclude(x => x.TicketStatus)
                .Include(p => p.Tickets).ThenInclude(x => x.TicketPriority)
                .Include(p => p.Members)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            if(user is not null) if(project.CompanyId != user.CompanyId) return NotFound();
            return View(project);
        }

        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            AddProjectWithPMViewModel model = new()
            {
                PMList = new SelectList(await _rolesService.GetUsersInRoleAsync("Project Manager", companyId), "Id", "FullName"),
                PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name")
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyId,Name,Description,StartDate,EndDate,ProjectPriorityId,ImageFileName,ImageFileData,ImageContentType,Archived")] Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,StartDate,EndDate,ProjectPriorityId,ImageFileName,ImageFileData,ImageContentType,Archived")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
