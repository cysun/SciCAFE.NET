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

        [Display(Name = "Event Reviewer")]
        public bool IsEventReviewer { get; set; }

        [Display(Name = "Reward Provider")]
        public bool IsRewardProvider { get; set; }

        [Display(Name = "Reward Reviewer")]
        public bool IsRewardReviewer { get; set; }
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

    public class ProfileInputModel
    {
        [Display(Name ="CIN")]
        public string Cin { get; set; }

        public string Major { get; set; }

        [Display(Name = "Program Affiliation(s)")]
        public List<int> ProgramIds { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class EventInputModel
    {
        [Required]
        [Display(Name = "Event Name")]
        [StringLength(80, ErrorMessage = "The {0} value cannot exceed {1} characters. ")]
        public string Name { get; set; }

        public string Location { get; set; }

        [Required]
        public string Description { get; set; }

        [Display(Name = "Start Time")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; } = DateTime.Today;

        public int LengthHours { get; set; } = 1;
        public int LengthMinutes { get; set; }

        [Display(Name = "Please choose the category that best describes the event")]
        public int? CategoryId { get; set; }

        [Display(Name = "Program Affiliation(s)")]
        public List<int> ProgramIds { get; set; }

        [Display(Name = "Who is your target audience?")]
        public string TargetAudience { get; set; }
    }

    public class RewardInputModel
    {
        [Required]
        [Display(Name = "Reward Name")]
        [StringLength(80, ErrorMessage = "The {0} value cannot exceed {1} characters. ")]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Expiration Date")]
        public DateTime? ExpireDate { get; set; }

        [Required]
        public string Description { get; set; }
    }

    public class EmailInputModel
    {
        public string To { get; set; }

        public string RedirectUrl { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
