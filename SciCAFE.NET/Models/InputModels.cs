using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SciCAFE.NET.Models
{
    public class LoginInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
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

    public class NewUserInputModel : RegistrationInputModel
    {
        [Display(Name = "Administrator")]
        public bool IsAdministrator { get; set; }

        [Display(Name = "Event Organizer")]
        public bool IsEventOrganizer { get; set; }

        [Display(Name = "Reward Provider")]
        public bool IsRewardProvider { get; set; }
    }

    public class EditUserInputModel : NewUserInputModel
    {
        [DataType(DataType.Password)]
        new public string Password { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        public string Name => $"{FirstName} {LastName}";
    }

    public class ProgramInputModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Short Name")]
        [StringLength(50, ErrorMessage = "The {0} value cannot exceed {1} characters. ")]
        public string ShortName { get; set; }

        public string Description { get; set; }

        [MaxLength(255)]
        [DataType(DataType.Url)]
        public string Website { get; set; }
    }
}
