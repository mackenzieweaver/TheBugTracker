using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace TheBugTracker.Models
{
    public class BTUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [NotMapped]
        [Display(Name = "Full Name")]
        public string FullName { get => $"{FirstName} {LastName}"; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile AvatarFormFile { get; set; }

        [DisplayName("Avatar")]
        public string AvatarFileName { get; set; }

        public byte[] AvatarFileData { get; set; }

        [DisplayName("File Extension")]
        public string AvatarFileContentType { get; set; }

        public int CompanyId { get; set; }

        // Navigation Properties
        public virtual Company Company { get; set; }
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
        public virtual ICollection<TicketComment> TicketComments { get; set; } = new HashSet<TicketComment>();
        public virtual ICollection<TicketAttachment> TicketAttachments { get; set; } = new HashSet<TicketAttachment>();
    }
}
