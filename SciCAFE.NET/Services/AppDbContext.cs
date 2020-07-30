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

        public DbSet<Tag> Tags { get; set; }
        public DbSet<Models.Program> Programs { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Reward> Rewards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tag>().HasAlternateKey(t => t.Name);
            modelBuilder.Entity<Models.Program>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Models.Program>().Property(p => p.IsDeleted).HasDefaultValue(false);
            modelBuilder.Entity<Event>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<EventTag>().HasKey(e => new { e.EventId, e.TagId });
            modelBuilder.Entity<Attendance>().HasAlternateKey(a => new { a.EventId, a.UserId });
            modelBuilder.Entity<Reward>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<Reward>().Property(r => r.NumOfEventsToQualify).HasDefaultValue(1);
            modelBuilder.Entity<RewardEvent>().HasKey(r => new { r.RewardId, r.EventId });
        }
    }
}
