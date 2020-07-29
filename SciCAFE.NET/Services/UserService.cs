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

        public User Authenticate(string email, string password)
        {
            var user = GetUser(email);
            return user != null && BCrypt.Net.BCrypt.Verify(password, user.Hash) ? user : null;
        }

        public User GetUser(string email)
        {
            return _db.Users.Where(u => u.Email.ToLower() == email.ToLower()).SingleOrDefault();
        }

        public void AddUser(User user) => _db.Users.Add(user);

        public void SaveChanges() => _db.SaveChanges();
    }
}
