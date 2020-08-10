using System;

namespace SciCAFE.NET.Models
{
    public class StatusViewModel
    {
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    public class ErrorViewModel
    {
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    public class EventViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public string StartTimeString => StartTime.ToString("g");
        public DateTime EndTime { get; set; }
        public string EndTimeString => EndTime.ToString("g");
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
