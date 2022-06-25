using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TheBugTracker.Models.ViewModels
{
    public class AddUsersToProjectViewModel
    {
        public int ProjectId { get; set; }
        public MultiSelectList Users { get; set; }
        public List<string> UserIds { get; set; }
    }
}