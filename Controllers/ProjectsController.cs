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
using TheBugTracker.Models.Enums;
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
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _projectService;

        public ProjectsController(ApplicationDbContext context, IBTRolesService rolesService, IBTLookupService lookupService, IBTFileService fileService, IBTProjectService projectService)
        {
            _context = context;
            _rolesService = rolesService;
            _lookupService = lookupService;
            _fileService = fileService;
            _projectService = projectService;
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
                PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName"),
                PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name")
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            var file = model.Project.ImageFormFile;
            if(file is not null)
            {
                model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(file);
                model.Project.ImageFileName = file.FileName;
                model.Project.ImageContentType = file.ContentType;
            }
            model.Project.CompanyId = companyId;
            model.Project.ProjectPriorityId = model.ProjectPriority;
            await _projectService.AddNewProjectAsync(model.Project);
            if(model.PMId is not null)
            {
                await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) return NotFound();

            int companyId = User.Identity.GetCompanyId().Value;
            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            if (project is null) return NotFound();
            
            AddProjectWithPMViewModel model = new()
            {
                Project = project,
                PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName"),
                PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name"),
                ProjectPriority = project.ProjectPriorityId.Value
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
        {
            var file = model.Project.ImageFormFile;
            if(file is not null)
            {
                model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(file);
                model.Project.ImageFileName = file.FileName;
                model.Project.ImageContentType = file.ContentType;
            }
            model.Project.ProjectPriorityId = model.ProjectPriority;
            await _projectService.UpdateProjectAsync(model.Project);
            if(model.PMId is not null)
            {
                await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
