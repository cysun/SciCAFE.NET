﻿using System;
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

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public List<EventProgram> EventPrograms { get; set; } = new List<EventProgram>();

        public List<EventTheme> EventThemes { get; set; } = new List<EventTheme>();

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

    public class Attendance
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}
