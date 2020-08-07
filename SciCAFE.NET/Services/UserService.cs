using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using SciCAFE.NET.Models;

namespace SciCAFE.NET.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public List<User> GetUsers()
        {
            return _db.Users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ToList();
        }

        public List<User> GetEventReviewers()
        {
            return _db.Users.Where(u => u.IsEventReviewer).ToList();
        }

        public User GetUser(string id)
        {
            return _db.Users.Find(id);
        }

        public void SaveChanges() => _db.SaveChanges();
    }
}
