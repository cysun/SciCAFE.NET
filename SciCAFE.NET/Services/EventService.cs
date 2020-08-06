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

        public List<Event> GetEventsByCreator(string creatorId)
        {
            return _db.Events.Where(e => e.CreatorId == creatorId).OrderByDescending(e => e.StartTime).ToList();
        }

        public Event GetEvent(int id)
        {
            return _db.Events.Where(e => e.Id == id)
                .Include(e => e.EventPrograms)
                .Include(e => e.EventThemes)
                .SingleOrDefault();
        }

        public void AddEvent(Event evnt) => _db.Events.Add(evnt);

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

        public void RemoveTheme(int eventId, int themeId)
        {
            var eventTheme = new EventTheme
            {
                EventId = eventId,
                ThemeId = themeId
            };
            _db.EventThemes.Remove(eventTheme);
        }

        public List<EventTheme> GetEventThemes(int eventId)
        {
            return _db.EventThemes.Where(t => t.EventId == eventId).ToList();
        }

        public void AddEventTheme(EventTheme eventTheme) => _db.EventThemes.Add(eventTheme);

        public void SaveChanges() => _db.SaveChanges();
    }
}
