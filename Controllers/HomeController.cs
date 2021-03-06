using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Controllers
{
    public class HomeController : Controller
    {
        private const int defaultSeedNumber = 5;
        
        private readonly IBTFileService _fileService;
        private readonly ISeedService _seedService;
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context,IBTFileService fileService,ISeedService seedService)
        {
            _fileService = fileService;
            _seedService = seedService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {            
            return View(await _context.Tickets.ToListAsync());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Users()
        {
            List<BTUser> model = await _context.Users.ToListAsync();
            return View("Users", model);
        }
        
        public IActionResult Chat()
        {
            return View();
        }
        
        public async Task<IActionResult> SeedCompanies([FromQuery] int number = defaultSeedNumber)
        {
            await _seedService.SeedCompanies(number);
            return RedirectToAction("Index", "Companies");
        }
        
        public async Task<IActionResult> UnseedCompanies()
        {
            await _seedService.UnseedCompanies();
            return RedirectToAction("Index", "Companies");
        }
        
        public async Task<IActionResult> SeedProjects([FromQuery] int number = defaultSeedNumber)
        {
            await _seedService.SeedProjects(number);
            return RedirectToAction("Index", "Projects");
        }
        
        public async Task<IActionResult> UnseedProjects()
        {
            await _seedService.UnseedProjects();
            return RedirectToAction("Index", "Projects");
        }
        
        public async Task<IActionResult> SeedTickets([FromQuery] int number = defaultSeedNumber)
        {
            await _seedService.SeedTickets(number);
            return RedirectToAction("Index", "Tickets");
        }
        
        public async Task<IActionResult> UnseedTickets()
        {
            await _seedService.UnseedTickets();
            return RedirectToAction("Index", "Tickets");
        }
        
        public async Task<IActionResult> SeedTicketTypes()
        {
            await _seedService.SeedTicketTypes();
            return RedirectToAction("Index", "TicketTypes");
        }
        
        public async Task<IActionResult> UnseedTicketTypes()
        {
            await _seedService.UnseedTicketTypes();
            return RedirectToAction("Index", "TicketTypes");
        }
        
        public async Task<IActionResult> SeedTicketStatuses()
        {
            await _seedService.SeedTicketStatuses();
            return RedirectToAction("Index", "TicketStatuses");
        }
        
        public async Task<IActionResult> UnseedTicketStatuses()
        {
            await _seedService.UnseedTicketStatuses();
            return RedirectToAction("Index", "TicketStatuses");
        }
        
        public async Task<IActionResult> SeedTicketPriorities()
        {
            await _seedService.SeedTicketPriorities();
            return RedirectToAction("Index", "TicketPriorities");
        }
        
        public async Task<IActionResult> UnseedTicketPriorities()
        {
            await _seedService.UnseedTicketPriorities();
            return RedirectToAction("Index", "TicketPriorities");
        }

        public async Task<IActionResult> SeedProjectPriorities()
        {
            await _seedService.SeedProjectPriorities();
            return RedirectToAction("Index", "ProjectPriorities");
        }
        
        public async Task<IActionResult> UnseedProjectPriorities()
        {
            await _seedService.UnseedProjectPriorities();
            return RedirectToAction("Index", "ProjectPriorities");
        }
        
        public async Task<IActionResult> SeedInvites([FromQuery] int number = defaultSeedNumber)
        {
            await _seedService.SeedInvites(number);
            return RedirectToAction("Index", "Invites");
        }
        
        public async Task<IActionResult> UnseedInvites()
        {
            await _seedService.UnseedInvites();
            return RedirectToAction("Index", "Invites");
        }
        
        public async Task<IActionResult> SeedTicketHistories([FromQuery] int number = defaultSeedNumber)
        {
            await _seedService.SeedTicketHistories(number);
            return RedirectToAction("Index", "Tickets");
        }
        
        public async Task<IActionResult> UnseedTicketHistories()
        {
            await _seedService.UnseedTicketHistories();
            return RedirectToAction("Index", "Tickets");
        }
    }
}
