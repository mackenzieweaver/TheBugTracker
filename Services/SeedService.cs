using System.IO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace TheBugTracker.Services
{
    public class SeedService : ISeedService
    {        
        private const int defaultSeedNumber = 5;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IConfiguration _configuration;

        public SeedService(ApplicationDbContext context, UserManager<BTUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task SeedAll()
        {
            await SeedUsers();
            await SeedInvites();
            await SeedCompanies();
            await SeedTicketTypes();
            await SeedTicketStatuses();
            await SeedTicketPriorities();
            await SeedProjectPriorities();
            await SeedProjects();
            await SeedTickets();
            
            await SeedTicketComments();
            await SeedTicketHistories();
            await SeedNotifications();
        }

        public async Task SeedCompanies(int number = defaultSeedNumber)
        {
            string path = Path.Combine("Data", "jobs.json");
            string json = File.ReadAllText(path);
            List<Job> jobs = JsonSerializer.Deserialize<List<Job>>(json);
            List<int> usedIndices = new List<int>();
            for (int i = 0; i < number; i++)
            {
                int x;
                int count = 0;
                do{
                    x = (new Random()).Next(0, jobs.Count);
                    if(++count > jobs.Count) break;
                } while(usedIndices.Contains(x));
                usedIndices.Add(x);
                Company company = new()
                {
                    Name = jobs[x].company_name,
                    Description = jobs[x].description
                };
                await _context.AddAsync(company);
            }
            await _context.SaveChangesAsync();
        }

        public async Task UnseedCompanies()
        {
            List<Project> projects = await _context.Projects.Where(p => p.CompanyId != null).ToListAsync();
            if(projects.Count > 0)
            {
                foreach(var project in projects)
                {
                    _context.Remove(project);
                }
                await _context.SaveChangesAsync();
            }

            List<Company> list = await _context.Companies.ToListAsync();
            if(list is null) return;
            foreach(var x in list)
            {
                _context.Remove(x);
            }
            await _context.SaveChangesAsync();
        }

        public async Task SeedInvites()
        {
            throw new System.NotImplementedException();
        }

        public async Task SeedNotifications()
        {
            throw new System.NotImplementedException();
        }

        public async Task SeedProjectPriorities()
        {
            await _context.AddAsync(new ProjectPriority { Name = "Very Low" });
            await _context.AddAsync(new ProjectPriority { Name = "Low" });
            await _context.AddAsync(new ProjectPriority { Name = "Normal" });
            await _context.AddAsync(new ProjectPriority { Name = "High" });
            await _context.AddAsync(new ProjectPriority { Name = "Very High" });
            await _context.SaveChangesAsync();
        }

        public async Task UnseedProjectPriorities()
        {
            var list = await _context.ProjectPriorities.ToListAsync();
            if(list is null) return;
            foreach(var x in list)
            {
                _context.Remove(x);
            }
            await _context.SaveChangesAsync();
        }

        public async Task SeedProjects(int number = defaultSeedNumber)
        {
            int projectsPerPage = 20;
            int projectCount = _context.Projects.Count();
            
            var page = (float) projectCount / (projectsPerPage - 1);
            string url = "https://github.com/marketplace?type=apps";
            if(page > 1) url = string.Concat(url, $"&page={Math.Ceiling(page)}");

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//*[@id='js-pjax-container']/div[2]/div[1]/div[2]/div/a");
            
            int nextProjectIndex = projectCount % projectsPerPage;
            int max = nextProjectIndex + number;
            for (int i = nextProjectIndex; i < max; i++)
            {
                if(nodes.Count < projectsPerPage) projectsPerPage = nodes.Count;
                if(i >= projectsPerPage)
                {
                    max -= i;
                    i = 0;
                    page += 1;
                    url = $"https://github.com/marketplace?type=apps&page={Math.Ceiling(page)}";                    
                    doc = web.Load(url);
                    nodes = doc.DocumentNode.SelectNodes("//*[@id='js-pjax-container']/div[2]/div[1]/div[2]/div/a");
                }

                var name = nodes[i].ChildNodes[3].ChildNodes[1].ChildNodes[0].InnerText.Trim();
                var description = nodes[i].ChildNodes[3].ChildNodes[5].InnerText.Trim();
                var image = nodes[i].ChildNodes[1].ChildNodes[1].ChildNodes[0].Attributes[2].Value;

                bool shouldHaveCompany = (new Random()).Next(0, 10) > 5 ? true : false;
                List<int> companyIds = companyIds = await _context.Companies.Select(c => c.Id).ToListAsync();
                if(companyIds is null || companyIds.Count == 0) shouldHaveCompany = false;
                var startDate = DateTime.Now.AddDays((new Random()).Next(0, 365));

                Project project = new()
                {
                    Name = name,
                    Description = description,
                    ImageFileName = image,
                    CompanyId = shouldHaveCompany ? companyIds[(new Random()).Next(0, companyIds.Count)] : null,
                    StartDate = startDate,
                    EndDate = startDate.AddDays((new Random()).Next(0, 30))
                };
                await _context.AddAsync(project);
            }
            try
            {
                await _context.SaveChangesAsync();                
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
                foreach(var project in _context.ChangeTracker.Entries())
                {
                    Console.WriteLine(project);                    
                }
            }
        }

        public async Task UnseedProjects()
        {
            List<Project> list = await _context.Projects.ToListAsync();
            if(list is null) return;
            foreach(var x in list)
            {
                _context.Remove(x);
            }
            await _context.SaveChangesAsync();
        }
        
        public async Task SeedTicketAttachments(int ticketId, string ticketUrl)
        {
            string url = "https://catalog.data.gov/dataset" + ticketUrl;
        }

        public async Task SeedTicketComments()
        {
            throw new System.NotImplementedException();
        }

        public async Task SeedTicketHistories()
        {
            throw new System.NotImplementedException();
        }

        public async Task SeedTicketPriorities()
        {
            await _context.AddAsync(new TicketPriority { Name = "Very Low" });
            await _context.AddAsync(new TicketPriority { Name = "Low" });
            await _context.AddAsync(new TicketPriority { Name = "Normal" });
            await _context.AddAsync(new TicketPriority { Name = "High" });
            await _context.AddAsync(new TicketPriority { Name = "Very High" });
            await _context.SaveChangesAsync();
        }
        
        public async Task UnseedTicketPriorities()
        {
            var list = await _context.TicketPriorities.ToListAsync();
            if(list is null) return;
            foreach(var x in list)
            {
                _context.Remove(x);
            }
            await _context.SaveChangesAsync();
        }

        public async Task SeedTickets(int number = defaultSeedNumber)
        {
            int page = 1;
            string url = $"https://catalog.data.gov/dataset?page={page}";

            int numberOfTicketsInDb = _context.Tickets.Count();

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//*[@id='content']/div[2]/div/section[1]/div[2]/ul/li");
            
            int numberOfTicketsOnPage = nodes.Count;
            int nextTicketIndex = numberOfTicketsInDb % numberOfTicketsOnPage;
            int max = nextTicketIndex + number;
            
            var projectIds = await _context.Projects.Select(x => x.Id).ToListAsync();
            var ticketTypeIds = await _context.TicketTypes.Select(x => x.Id).ToListAsync();
            var ticketStatusIds = await _context.TicketStatuses.Select(x => x.Id).ToListAsync();
            var ticketPriorityIds = await _context.TicketPriorities.Select(x => x.Id).ToListAsync();
            var userIds = await _context.Users.Select(x => x.Id).ToListAsync();

            Dictionary<int, string> ticketIdsAndUrls = new();
            for (int i = nextTicketIndex; i < max; i++)
            {
                if(i >= numberOfTicketsOnPage)
                {
                    max -= i;
                    i = 0;
                    page += 1;
                    url = $"https://catalog.data.gov/dataset?page={page}";
                    doc = web.Load(url);
                    nodes = doc.DocumentNode.SelectNodes("//*[@id='content']/div[2]/div/section[1]/div[2]/ul/li");
                }

                string title = nodes[i].ChildNodes[1].ChildNodes[3].ChildNodes[1].InnerText;
                if(title.Length > 50) title = title.Substring(0, 47) + "...";
                string description = nodes[i].ChildNodes[1].ChildNodes[5].ChildNodes[3].InnerText;
                DateTimeOffset created = DateTime.Now.Add(new TimeSpan(
                    days: (new Random()).Next(0, 365), 
                    hours: (new Random()).Next(0, 60), 
                    minutes: (new Random()).Next(0, 60), 
                    seconds: (new Random()).Next(0, 60))
                );
                bool archived = (new Random()).Next(0, 10) < 5 ? true : false;

                if(_context.Projects.Count() == 0) await SeedProjects();
                int projectId = projectIds[(new Random()).Next(0, _context.Projects.Count())];
                
                if(_context.TicketTypes.Count() == 0) await SeedTicketTypes();
                int ticketTypeId = ticketTypeIds[(new Random()).Next(0, _context.TicketTypes.Count())];
                
                if(_context.TicketStatuses.Count() == 0) await SeedTicketTypes();
                int ticketStatusId = ticketStatusIds[(new Random()).Next(0, _context.TicketStatuses.Count())];
                
                if(_context.TicketPriorities.Count() == 0) await SeedTicketTypes();
                int ticketPriorityId = ticketPriorityIds[(new Random()).Next(0, _context.TicketPriorities.Count())];
                
                if(_context.Users.Count() == 0) await SeedUsers();
                string ownerUserId = userIds[(new Random()).Next(0, _context.Users.Count())];
                string developerUserId = userIds[(new Random()).Next(0, _context.Users.Count())];

                Ticket ticket = new()
                {
                    Title = title,
                    Description = description,
                    Created = created,
                    Updated = null,
                    Archived = archived,
                    ProjectId = projectId,
                    TicketTypeId = ticketTypeId,
                    TicketStatusId = ticketStatusId,
                    TicketPriorityId = ticketPriorityId,
                    OwnerUserId = ownerUserId,
                    DeveloperUserId = developerUserId
                };
                await _context.AddAsync(ticket);
                await _context.SaveChangesAsync();
                ticketIdsAndUrls.Add(ticket.Id, nodes[i].ChildNodes[1].ChildNodes[3].ChildNodes[1].Attributes[0].Value);
            }

            foreach (var item in ticketIdsAndUrls)
            {
                await SeedTicketAttachments(item.Key, item.Value);
            }
        }

        public async Task UnseedTickets()
        {
            List<Ticket> list = await _context.Tickets.ToListAsync();
            if(list is null) return;
            foreach(var x in list)
            {
                _context.Remove(x);
            }
            await _context.SaveChangesAsync();
        }

        public async Task SeedTicketStatuses()
        {
            await _context.AddAsync(new TicketStatus { Name = "Backlog" });
            await _context.AddAsync(new TicketStatus { Name = "Scope" });
            await _context.AddAsync(new TicketStatus { Name = "In Progress" });
            await _context.AddAsync(new TicketStatus { Name = "UAT" });
            await _context.AddAsync(new TicketStatus { Name = "RTD" });
            await _context.AddAsync(new TicketStatus { Name = "Done" });
            await _context.SaveChangesAsync();
        }

        public async Task UnseedTicketStatuses()
        {
            var list = await _context.TicketStatuses.ToListAsync();
            if(list is null) return;
            foreach(var x in list)
            {
                _context.Remove(x);
            }
            await _context.SaveChangesAsync();
        }

        public async Task SeedTicketTypes()
        { 
            await _context.AddAsync(new TicketType { Name = "Frontend" });
            await _context.AddAsync(new TicketType { Name = "Backend" });
            await _context.AddAsync(new TicketType { Name = "Fullstack" });
            await _context.AddAsync(new TicketType { Name = "Database" });
            await _context.AddAsync(new TicketType { Name = "Api" });
            await _context.AddAsync(new TicketType { Name = "Test" });
            await _context.AddAsync(new TicketType { Name = "QA" });
            await _context.AddAsync(new TicketType { Name = "DevOps" });
            await _context.SaveChangesAsync();
        }

        public async Task UnseedTicketTypes()
        {
            var list = await _context.TicketTypes.ToListAsync();
            if(list is null) return;
            foreach(var x in list)
            {
                _context.Remove(x);
            }
            await _context.SaveChangesAsync();
        }

        public async Task SeedUsers(int number = defaultSeedNumber)
        {
            string seededUserPassword = "Pa$$w0rd";
            string url = "https://randomuser.me/api/";
            for (int i = 0; i < number; i++)
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(url);
                string json = await response.Content.ReadAsStringAsync();
                try
                {
                    Root root = JsonSerializer.Deserialize<Root>(json);
                    BTUser user = new BTUser
                    {
                        UserName = root.results[0].login.username,
                        Email = root.results[0].email,
                        FirstName = root.results[0].name.first,
                        LastName = root.results[0].name.last,
                        AvatarFileName = root.results[0].picture.thumbnail,
                        EmailConfirmed = true
                    };
                    var result = await _userManager.CreateAsync(user, seededUserPassword);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    i--; // retry
                }
                client.Dispose();
            }
        }

        public async Task UnseedUsers()
        {
            List<BTUser> users = await _context.Users.ToListAsync();
            if(users is null) return;
            foreach(BTUser user in users)
            {
                _context.Remove(user);
            }
            await _context.SaveChangesAsync();
        }
    }
}
