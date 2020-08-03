using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public List<Category> GetCategories()
        {
            return _db.Categories.OrderBy(c => c.Name).ToList();
        }

        public void AddCategory(Category category) => _db.Categories.Add(category);


        public void SaveChanges() => _db.SaveChanges();
    }
}
