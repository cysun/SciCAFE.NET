using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SciCAFE.NET.Models;
using SciCAFE.NET.Services;

namespace ConsoleManager
{
    public class ConsoleManager
    {
        readonly ServiceProvider serviceProvider;

        AppDbContext db => serviceProvider.GetService<AppDbContext>();

        public ConsoleManager()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../SciCAFE.NET"))
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
            serviceProvider = services.BuildServiceProvider();
        }

        public void MainController()
        {
            var done = false;
            do
            {
                var cmd = MainView();
                switch (cmd)
                {
                    case "a":
                        AddUser();
                        break;
                    case "x":
                        done = true;
                        break;
                }
            } while (!done);

            serviceProvider.Dispose();
        }

        private string MainView()
        {
            var validChoices = new HashSet<string>() { "a", "x" };
            string choice;
            do
            {
                Console.Clear();
                Console.WriteLine("\t Main Menu \n");
                Console.WriteLine("\t a) Add A User");
                Console.WriteLine("\t x) Exit");
                Console.Write("\n  Pleasse enter your choice: ");
                choice = Console.ReadLine().ToLower();
            } while (!validChoices.Contains(choice));

            return choice;
        }

        private void AddUser()
        {
            Console.Clear();
            Console.WriteLine("\t Add User \n");
            var user = new User();
            Console.Write("\t First Name: ");
            user.FirstName = Console.ReadLine();
            Console.Write("\t Last Name: ");
            user.LastName = Console.ReadLine();
            Console.Write("\t Email: ");
            user.Email = Console.ReadLine().ToLower();
            Console.Write("\t Password: ");
            user.Hash = BCrypt.Net.BCrypt.HashPassword(Console.ReadLine());
            Console.Write("\t Administrator? [y|n]: ");
            user.IsAdministrator = Console.ReadLine().ToLower() == "y";
            Console.Write("\t Event Organizer? [y|n]: ");
            user.IsEventOrganizer = Console.ReadLine().ToLower() == "y";
            Console.Write("\t Reward Provider? [y|n]: ");
            user.IsRewardProvider = Console.ReadLine().ToLower() == "y";
            Console.Write("\t Save or Cancel? [s|c] ");
            var cmd = Console.ReadLine().ToLower();
            if (cmd == "s")
            {
                db.Users.Add(user);
                db.SaveChanges();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            (new ConsoleManager()).MainController();
        }
    }
}
