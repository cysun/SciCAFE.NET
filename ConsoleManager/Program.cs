using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SciCAFE.NET.Models;
using SciCAFE.NET.Security;
using SciCAFE.NET.Security.Constants;
using SciCAFE.NET.Services;

namespace ConsoleManager
{
    public class ConsoleManager
    {
        readonly ServiceProvider serviceProvider;

        UserManager<User> userManager => serviceProvider.GetService<UserManager<User>>();

        public ConsoleManager()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../SciCAFE.NET"))
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            serviceProvider = services.BuildServiceProvider();
        }

        public async Task MainControllerAsync()
        {
            var done = false;
            do
            {
                var cmd = MainView();
                switch (cmd)
                {
                    case "a":
                        await AddUserAsync();
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

        private async Task AddUserAsync()
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
            user.UserName = user.Email;
            Console.Write("\t Password: ");
            var password = Console.ReadLine();
            Console.Write("\t Administrator? [y|n]: ");
            user.IsAdministrator = Console.ReadLine().ToLower() == "y";
            Console.Write("\t Event Organizer? [y|n]: ");
            user.IsEventOrganizer = Console.ReadLine().ToLower() == "y";
            Console.Write("\t Event Reviewer? [y|n]: ");
            user.IsEventReviewer = Console.ReadLine().ToLower() == "y";
            Console.Write("\t Reward Provider? [y|n]: ");
            user.IsRewardProvider = Console.ReadLine().ToLower() == "y";
            Console.Write("\t Reward Reviewer? [y|n]: ");
            user.IsRewardReviewer = Console.ReadLine().ToLower() == "y";
            Console.Write("\t Save or Cancel? [s|c] ");
            var cmd = Console.ReadLine().ToLower();
            if (cmd == "s")
            {
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    var claims = SecurityUtils.GetAdditionalClaims(user);
                    if (claims.Count > 0)
                        await userManager.AddClaimsAsync(user, claims);

                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    await userManager.ConfirmEmailAsync(user, token);
                }
                else
                {
                    Console.WriteLine("\n\t Failed to create the user");
                    foreach (var error in result.Errors)
                        Console.WriteLine($"\t {error.Description}");
                    Console.Write("\n\n\t Press [Enter] key to continue");
                    Console.ReadLine();
                }
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            await (new ConsoleManager()).MainControllerAsync();
        }
    }
}
