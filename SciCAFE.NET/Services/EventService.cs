using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SciCAFE.NET.Models;

namespace SciCAFE.NET.Services
{
    public class EventService
    {
        private readonly AppDbContext _db;

        public EventService(AppDbContext db)
        {
            _db = db;
        }

        public List<Event> GetUpcomingEvents()
        {
            return _db.Events.Where(e =>
                DateTime.Now < e.StartTime && e.StartTime < DateTime.Now.AddDays(14) && e.Review.IsApproved == true)
                .OrderBy(e => e.StartTime)
                .ToList();
        }

        public List<Event> GetEvents(DateTime startTime, DateTime endTime)
        {
            return _db.Events.Where(e =>
                startTime <= e.StartTime && e.EndTime < endTime && e.Review.IsApproved == true)
                .OrderBy(e => e.StartTime)
                .ToList();
        }

        public List<Event> GetUnreviewedEvents()
        {
            return _db.Events.Where(e => e.SubmitDate != null && e.Review.IsApproved == null)
                .Include(e => e.Creator)
                .OrderBy(e => e.SubmitDate)
                .ToList();
        }

        public List<Event> GetEventsByCreator(string creatorId)
        {
            return _db.Events.Where(e => e.CreatorId == creatorId).OrderByDescending(e => e.StartTime).ToList();
        }

        public List<Event> SearchEvents(string term)
        {
            return _db.Events.FromSqlRaw("SELECT * FROM \"SearchEvents\"({0})", term?.ToLower()).ToList();
        }

        public Event GetEvent(int id)
        {
            return _db.Events.Where(e => e.Id == id)
                .Include(e => e.Creator)
                .Include(e => e.Category)
                .Include(e => e.EventPrograms).ThenInclude(p => p.Program)
                .Include(e => e.EventThemes).ThenInclude(t => t.Theme)
                .SingleOrDefault();
        }

        public void AddEvent(Event evnt) => _db.Events.Add(evnt);

        public List<Attendance> GetEventAttendances(int eventId)
        {
            return _db.Attendances.Where(a => a.EventId == eventId)
                .Include(a => a.Attendee)
                .OrderBy(a => a.Attendee.FirstName).ThenBy(a => a.Attendee.LastName)
                .ToList();
        }

        public Attendance GetAttendance(int eventId, string attendeeId)
        {
            return _db.Attendances.Where(a => a.EventId == eventId && a.AttendeeId == attendeeId)
                .SingleOrDefault();
        }

        public void AddAttendance(Attendance attendance) => _db.Attendances.Add(attendance);

        public void RemoveAttendance(Attendance attendance) => _db.Attendances.Remove(attendance);

        public List<Category> GetCategories()
        {
            return _db.Categories.ToList();
        }

        public Category GetCategory(int id)
        {
            return _db.Categories.Find(id);
        }

        public void AddCategory(Category category) => _db.Categories.Add(category);

        public List<Theme> GetThemes()
        {
            return _db.Themes.OrderBy(c => c.Name).ToList();
        }

        public Theme GetTheme(int id)
        {
            return _db.Themes.Find(id);
        }

        public void AddTheme(Theme theme) => _db.Themes.Add(theme);

        public void SaveChanges() => _db.SaveChanges();
    }
}
