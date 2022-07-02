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
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Controllers
{
    [Authorize]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTCompanyInfoService _companyInfoService;

        public CompaniesController(ApplicationDbContext context, IBTCompanyInfoService companyInfoService)
        {
            _context = context;
            _companyInfoService = companyInfoService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var companies = await _context.Companies
                .Include(x => x.Projects)
                .Include(x => x.Members)
                .ToListAsync();
            return View(companies);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Profile(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            if(user is not null) if(user.CompanyId != id) return NotFound();

            var company = await _context.Companies
                .Include(x => x.Members)
                .Include(x => x.Projects).ThenInclude(x => x.ProjectPriority)
                .FirstOrDefaultAsync(x => x.Id == id);
            return View(new CompanyProfileViewModel { Company = company, Members = company.Members, Projects = company.Projects });
        }

        [AllowAnonymous]
        public async Task<IActionResult> AddProjectToCompany(int companyId)
        {
            var projects = await _context.Projects.ToListAsync();
            var projectsWithoutCompany = projects.Where(p => p.CompanyId == null).ToList();
            var projectsNotAlreadyInCompany = projects.Where(p => p.CompanyId != companyId).ToList();

            var project = projectsWithoutCompany.Count > 0 ? projectsWithoutCompany[(new Random()).Next(0, projectsWithoutCompany.Count)] :
                projectsNotAlreadyInCompany[(new Random()).Next(0, projectsNotAlreadyInCompany.Count)];

            project.CompanyId = companyId;
            _context.Update(project);
            await _context.SaveChangesAsync();
            return RedirectToAction("Profile", new { id = companyId });
        }
        
        [AllowAnonymous]
        public async Task<IActionResult> AddMemberToCompany(int? companyId)
        {
            var users = await _context.Users.ToListAsync();
            var usersWithoutCompany = users.Where(u => u.CompanyId == null).ToList();
            var usersNotAlreadyInCompany = users.Where(u => u.CompanyId != companyId).ToList();
            
            var user = usersWithoutCompany.Count > 0 ? usersWithoutCompany[(new Random()).Next(0, usersWithoutCompany.Count)] :
                usersNotAlreadyInCompany[(new Random()).Next(0, usersNotAlreadyInCompany.Count)];
                
            user.CompanyId = companyId.Value;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Profile", new { id = companyId });
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Company company)
        {
            if (ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
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
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
    }
}
