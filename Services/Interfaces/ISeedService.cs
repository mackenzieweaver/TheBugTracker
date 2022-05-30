using System.Threading.Tasks;

namespace TheBugTracker.Services.Interfaces
{
    public interface ISeedService
    {
        Task SeedAll();

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

        
        Task SeedTicketComments(int ticketId);
        Task UnseedTicketComments();

        Task SeedNotifications(int ticketId, string senderId, string title, string message);
        
        Task SeedTicketAttachments(int ticketId, string ticketUrl);
        
        Task SeedTicketHistories(int number);
        Task UnseedTicketHistories();
    }
}