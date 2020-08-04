using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SciCAFE.NET.Models
{
    public class User : IdentityUser
    {
        [Required, MaxLength(255)]
        public string FirstName { get; set; }

        [Required, MaxLength(255)]
        public string LastName { get; set; }

        public bool IsAdministrator { get; set; }
        public bool IsEventOrganizer { get; set; }
        public bool IsEventReviewer { get; set; }
        public bool IsRewardProvider { get; set; }
        public bool IsRewardReviewer { get; set; }

        public string Name => $"{FirstName} {LastName}";
    }
}
