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
            List<BTUser> model = await _context.Users.ToListAsync();
            return View(model);
        }
        
        public async Task<IActionResult> Profile(string id)
        {
            var tickets = await _context.Tickets
                .Include(x => x.OwnerUser)
                .Include(x => x.DeveloperUser)
                .Include(x => x.Project)
                .Include(x => x.TicketType)
                .Include(x => x.TicketStatus)
                .Include(x => x.TicketPriority)
                .ToListAsync();

            ProfileViewModel vm = new();
            vm.TicketsSubmitted = tickets.Where(t => t.OwnerUserId == id).ToList();
            vm.TicketsAssigned = tickets.Where(t => t.DeveloperUserId == id).ToList();
            vm.User = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

            return View(vm);
        }
    }
}