using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SciCAFE.NET.Models
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(255)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(255)]
        public string LastName { get; set; }

        public bool IsAdministrator { get; set; }
        public bool IsEventOrganizer { get; set; }
        public bool IsEventReviewer { get; set; }
        public bool IsRewardProvider { get; set; }
        public bool IsRewardReviewer { get; set; }

        public string Name => $"{FirstName} {LastName}";

        [MaxLength(255)]
        public string Cin { get; set; }

        [MaxLength(255)]
        public string Major { get; set; }

        public List<UserProgram> UserPrograms { get; set; }
    }

    [Table("UserPrograms")]
    public class UserProgram
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public int ProgramId { get; set; }
        public Models.Program Program { get; set; }
    }

}
