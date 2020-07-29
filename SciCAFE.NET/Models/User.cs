using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
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

        [MaxLength(255)]
        public string Hash { get; set; }

        public bool IsAdministrator { get; set; }
        public bool IsEventOrganizer { get; set; }
        public bool IsRewardProvider { get; set; }

        public bool Enabled { get; set; } = true;

        public List<Claim> ToClaims()
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Id.ToString()),
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimTypes.GivenName, FirstName),
                new Claim(ClaimTypes.Surname, LastName),
                new Claim("IsAdministrator", IsAdministrator.ToString()),
                new Claim("IsEventOrganizor", IsEventOrganizer.ToString()),
                new Claim("IsRewardProvider", IsRewardProvider.ToString())
            };
        }
    }
}
