using System.Threading.Tasks;

namespace TheBugTracker.Services.Interfaces
{
    public interface ISeedService
    {
        Task SeedAll();
        
        Task SeedNotifications();
        Task SeedTicketAttachments(int ticketId, string ticketUrl);
        Task SeedTicketComments();
        Task SeedTicketHistories();

        Task SeedUsers(int number);
        Task UnseedUsers();
        
        Task SeedCompanies(int number);
        Task UnseedCompanies();
        
        Task SeedProjects(int number);
        Task UnseedProjects();
        
        Task SeedTickets(int number);
        Task UnseedTickets();
        
        Task SeedTicketPriorities();
        Task UnseedTicketPriorities();
        
        Task SeedTicketStatuses();
        Task UnseedTicketStatuses();
        
        Task SeedTicketTypes();
        Task UnseedTicketTypes();
        
        Task SeedProjectPriorities();
        Task UnseedProjectPriorities();

        Task SeedInvites(int number);
        Task UnseedInvites();
    }
}