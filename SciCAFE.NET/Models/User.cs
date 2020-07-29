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

    public class RegistrationInputModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
