using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SciCAFE.NET.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(255)]
        public string FirstName { get; set; }

        [Required, MaxLength(255)]
        public string LastName { get; set; }

        [Required, MaxLength(255)]
        public string Email { get; set; }

        public bool IsAdmin { get; set; }
        public bool IsEventOrganizer { get; set; }
        public bool IsRewardProvider { get; set; }

        public bool Enabled { get; set; } = true;
    }
}
