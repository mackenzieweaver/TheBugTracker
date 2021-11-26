using Microsoft.AspNetCore.Mvc;
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
        private readonly ILogger<HomeController> _logger;
        private readonly IBTFileService _fileService;

        // private readonly IBTTicketHistoryService _historyService;
        // private readonly ApplicationDbContext _context;
        // private readonly IBTTicketService _ticketService;
        // private readonly IBTProjectService _projectService;

        public HomeController(ILogger<HomeController> logger,
            // IBTTicketHistoryService historyService, 
            // ApplicationDbContext context, 
            // IBTTicketService ticketService,
            // IBTProjectService projectService
            IBTFileService fileService
        )
        {
            _logger = logger;
            _fileService = fileService;
            // _historyService = historyService;
            // _context = context;
            // _ticketService = ticketService;
            // _projectService = projectService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
