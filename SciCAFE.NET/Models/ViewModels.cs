using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

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
        public int LengthHours { get; set; }
        public int LengthMintues { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class RewardeeViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public HashSet<int> AttendedEventIds { get; set; } = new HashSet<int>();

        public int NumOfEventsToQualify { get; set; }

        public bool RequirementsMet => AttendedEventIds.Count >= NumOfEventsToQualify;

        public string Name => $"{FirstName} {LastName}";
    }
}
