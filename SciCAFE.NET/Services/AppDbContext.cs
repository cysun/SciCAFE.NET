using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SciCAFE.NET.Models;

namespace SciCAFE.NET.Services
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Models.Program> Programs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Reward> Rewards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Models.Program>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Models.Program>().Property(p => p.IsDeleted).HasDefaultValue(false);

            modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Category>().Property(c => c.IsDeleted).HasDefaultValue(false);

            modelBuilder.Entity<Theme>().HasIndex(t => t.Name).IsUnique();
            modelBuilder.Entity<Theme>().HasQueryFilter(t => !t.IsDeleted);
            modelBuilder.Entity<Theme>().Property(t => t.IsDeleted).HasDefaultValue(false);

            modelBuilder.Entity<Event>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Event>().HasIndex(e => e.Name);

            modelBuilder.Entity<EventProgram>().HasKey(e => new { e.EventId, e.ProgramId });

            modelBuilder.Entity<EventTheme>().HasKey(e => new { e.EventId, e.ThemeId });

            modelBuilder.Entity<Attendance>().HasAlternateKey(a => new { a.EventId, a.AttendeeId });

            modelBuilder.Entity<Reward>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<Reward>().Property(r => r.NumOfEventsToQualify).HasDefaultValue(1);

            modelBuilder.Entity<RewardEvent>().HasKey(r => new { r.RewardId, r.EventId });
        }
    }
}
