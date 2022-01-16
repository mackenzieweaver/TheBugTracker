using System.Collections.Generic;

namespace TheBugTracker.Models
{
    public class CompanyProfileViewModel
    {
        public Company Company { get; set; }
        public ICollection<BTUser> Members { get; set; }
        public ICollection<Project> Projects { get; set; }
    }
}