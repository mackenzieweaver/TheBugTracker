using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly IBTCompanyInfoService _companyService;

        public ProjectsController(ApplicationDbContext context, IBTRolesService rolesService, IBTLookupService lookupService, IBTFileService fileService, IBTProjectService projectService, IBTCompanyInfoService companyService)
        {
            _context = context;
            _rolesService = rolesService;
            _lookupService = lookupService;
            _fileService = fileService;
            _projectService = projectService;
            _companyService = companyService;
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
            Project p = await _projectService.GetProjectByIdAsync(id, User.Identity.GetCompanyId().Value);
            if (p is null) return NotFound();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            if(user is not null) if(p.CompanyId != user.CompanyId) return NotFound();
            ViewData["PriorityList"] = new SelectList(_context.ProjectPriorities.Where(x => x.Id != p.ProjectPriorityId), "Id", "Name");
            return View(p);
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

        public async Task<IActionResult> Archive(int? id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Project p = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int Id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Project p = await _projectService.GetProjectByIdAsync(Id, companyId);
            await _projectService.ArchiveProjectAsync(p);
            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> Restore(int? id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Project p = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int Id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Project p = await _projectService.GetProjectByIdAsync(Id, companyId);
            await _projectService.RestoreProjectAsync(p);
            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> AddUsersToProject(int Id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            var users = await _companyService.GetAllMembersAsync(companyId);
            var usersOnProject = await _projectService.GetUsersOnProjectAsync(Id, companyId);
            
            AddUsersToProjectViewModel vm = new() {
                ProjectId = Id,
                Users = new MultiSelectList(users, "Id", "FullName", usersOnProject),
                UserIds = usersOnProject.Select(user => user.Id).ToList()
            };
            return View(vm);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUsersToProject(AddUsersToProjectViewModel vm)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            var users = await _companyService.GetAllMembersAsync(companyId);
            foreach (var id in users.Select(u => u.Id))
            {
                await _projectService.RemoveUserFromProjectAsync(id, vm.ProjectId);
            }
            foreach (var id in vm.UserIds)
            {
                await _projectService.AddUserToProjectAsync(id, vm.ProjectId);
            }
            return RedirectToAction("Details", new { id = vm.ProjectId });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditName(int id, string name)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Project p = await _projectService.GetProjectByIdAsync(id, companyId);
            p.Name = name;
            await _projectService.UpdateProjectAsync(p);
            return RedirectToAction("Details", new { id });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDescription(int id, string description)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Project p = await _projectService.GetProjectByIdAsync(id, companyId);
            p.Description = description;
            await _projectService.UpdateProjectAsync(p);
            return RedirectToAction("Details", new { id });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPriority(int id, int priorityId)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Project p = await _projectService.GetProjectByIdAsync(id, companyId);
            p.ProjectPriorityId = priorityId;
            await _projectService.UpdateProjectAsync(p);
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditArchived(int id, bool archive)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Project p = await _projectService.GetProjectByIdAsync(id, companyId);
            p.Archived = archive;
            await _projectService.UpdateProjectAsync(p);
            return RedirectToAction("Details", new { id });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStartDate(int id, DateTimeOffset StartDate)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Project p = await _projectService.GetProjectByIdAsync(id, companyId);
            p.StartDate = StartDate;
            await _projectService.UpdateProjectAsync(p);
            return RedirectToAction("Details", new { id });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEndDate(int id, DateTimeOffset EndDate)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Project p = await _projectService.GetProjectByIdAsync(id, companyId);
            p.EndDate = EndDate;
            await _projectService.UpdateProjectAsync(p);
            return RedirectToAction("Details", new { id });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditImage(int id, IFormFile ImageFormFile)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Project p = await _projectService.GetProjectByIdAsync(id, companyId);
            p.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(ImageFormFile);
            p.ImageFileName = ImageFormFile.FileName;
            p.ImageContentType = ImageFormFile.ContentType;
            await _projectService.UpdateProjectAsync(p);
            return RedirectToAction("Details", new { id });
        }
    }
}
