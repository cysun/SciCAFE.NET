using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SciCAFE.NET.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Location { get; set; }

        public string Description { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public List<EventTag> EventTags { get; set; } = new List<EventTag>();

        public string OrganizerId { get; set; }
        public User Organizer { get; set; }

        public DateTime? SubmitDate { get; set; }

        public Review Review { get; set; }

        public List<Attendance> Attendances { get; set; } = new List<Attendance>();

        public bool IsDeleted { get; set; }
    }

    [Table("EventTags")]
    public class EventTag
    {
        public int EventId { get; set; }
        public Event Event { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
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
