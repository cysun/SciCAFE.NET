using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SciCAFE.NET.Models;

namespace SciCAFE.NET.Services
{
    public class RewardService
    {
        private readonly AppDbContext _db;

        public RewardService(AppDbContext db)
        {
            _db = db;
        }

        public List<Reward> GetRecentRewards()
        {
            return _db.Rewards.Where(r => r.Review.IsApproved == true && (r.ExpireDate == null || r.ExpireDate > DateTime.Now)
                && r.SubmitDate > DateTime.Now.AddDays(-30))
                .OrderByDescending(r => r.SubmitDate)
                .ToList();
        }

        public List<Reward> GetUnreviewedRewards()
        {
            return _db.Rewards.Where(r => r.SubmitDate != null && r.Review.IsApproved == null)
                .Include(r => r.Creator)
                .OrderBy(e => e.SubmitDate)
                .ToList();
        }

        public List<Reward> GetRewardsByCreator(string creatorId)
        {
            return _db.Rewards.Where(e => e.CreatorId == creatorId).OrderByDescending(r => r.Id).ToList();
        }

        public void AddReward(Reward reward) => _db.Rewards.Add(reward);

        public void SaveChanges() => _db.SaveChanges();
    }
}
