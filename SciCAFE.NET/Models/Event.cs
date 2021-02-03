using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SciCAFE.NET.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        [Display(Name = "Additional Information")]
        public string AdditionalInfo { get; set; }

        public bool IsDeleted { get; set; }
    }

    public class Theme
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsDeleted { get; set; }
    }

    public class Event
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Location { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        public List<EventProgram> EventPrograms { get; set; } = new List<EventProgram>();

        public List<EventTheme> EventThemes { get; set; } = new List<EventTheme>();

        public List<EventAttachment> EventAttachments { get; set; } = new List<EventAttachment>();

        [MaxLength(255)]
        public string TargetAudience { get; set; }

        [MaxLength(255)]
        public string CoreCompetency { get; set; }

        public string CreatorId { get; set; }
        public User Creator { get; set; }

        public DateTime? SubmitDate { get; set; }

        public Review Review { get; set; }

        public List<Attendance> Attendances { get; set; } = new List<Attendance>();

        public bool IsDeleted { get; set; }

        public Event Clone()
        {
            var newEvent = new Event
            {
                Name = $"Copy of {Name}",
                Location = Location,
                Description = Description,
                StartTime = StartTime,
                EndTime = EndTime,
                CategoryId = CategoryId,
                TargetAudience = TargetAudience,
                CoreCompetency = CoreCompetency,
            };

            newEvent.EventPrograms = EventPrograms.Select(p => new EventProgram
            {
                Event = newEvent,
                ProgramId = p.ProgramId
            }).ToList();

            newEvent.EventThemes = EventThemes.Select(t => new EventTheme
            {
                Event = newEvent,
                ThemeId = t.ThemeId
            }).ToList();

            newEvent.EventAttachments = EventAttachments.Select(a => new EventAttachment
            {
                Event = newEvent,
                FileId = a.FileId
            }).ToList();

            return newEvent;
        }
    }

    [Table("EventPrograms")]
    public class EventProgram
    {
        public int EventId { get; set; }
        public Event Event { get; set; }

        public int ProgramId { get; set; }
        public Models.Program Program { get; set; }
    }

    [Table("EventThemes")]
    public class EventTheme
    {
        public int EventId { get; set; }
        public Event Event { get; set; }

        public int ThemeId { get; set; }
        public Theme Theme { get; set; }
    }

    [Table("EventAttachments")]
    public class EventAttachment
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        public int FileId { get; set; }
        public File File { get; set; }
    }

    public class Attendance
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        public string AttendeeId { get; set; }
        public User Attendee { get; set; }
    }
}
