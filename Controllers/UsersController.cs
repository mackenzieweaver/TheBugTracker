using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISeedService _seedService;
        private const int  defaultSeedNumber = 5;

        public UsersController(ApplicationDbContext context, ISeedService seedService)
        {
            _context = context;
            _seedService = seedService;
        }

        public async Task<IActionResult> Seed([FromQuery] int number = defaultSeedNumber)
        {
            await _seedService.SeedUsers(number);
            List<BTUser> model = await _context.Users.ToListAsync();
            return View("Index", model);
        }
        
        public async Task<IActionResult> Unseed()
        {
            await _seedService.UnseedUsers();
            return View("Index", new List<BTUser>());
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            var users = currentUser is not null ? 
                await _context.Users.Where(x => x.CompanyId == currentUser.CompanyId).ToListAsync() :
                await _context.Users.ToListAsync();
            return View(users);
        }
        
        public async Task<IActionResult> Profile(string id)
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            if(currentUser is not null)
            {
                var company = await _context.Companies.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == currentUser.CompanyId);
                if(company is null) return RedirectToAction(nameof(Index));
                if(!company.Members.Select(x => x.Id).Contains(id)) return RedirectToAction(nameof(Index));
            }

            var tickets = await _context.Tickets
                .Include(x => x.OwnerUser)
                .Include(x => x.DeveloperUser)
                .Include(x => x.Project)
                .Include(x => x.TicketType)
                .Include(x => x.TicketStatus)
                .Include(x => x.TicketPriority)
                .ToListAsync();

            UserProfileViewModel vm = new();
            vm.TicketsSubmitted = tickets.Where(t => t.OwnerUserId == id).ToList();
            vm.TicketsAssigned = tickets.Where(t => t.DeveloperUserId == id).ToList();
            vm.User = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

            return View(vm);
        }
    }
}