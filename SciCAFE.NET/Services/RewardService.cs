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

        public Reward GetReward(int id)
        {
            return _db.Rewards.Where(r => r.Id == id)
                .Include(r => r.Creator)
                .Include(r => r.RewardAttachments).ThenInclude(a => a.File)
                .Include(r => r.RewardEvents).ThenInclude(e => e.Event)
                .SingleOrDefault();
        }

        public List<Reward> GetRecentRewards()
        {
            return _db.Rewards.Where(r => r.Review.IsApproved == true && (r.ExpireDate == null || r.ExpireDate > DateTime.Now)
                && r.SubmitDate > DateTime.Now.AddDays(-30))
                .OrderByDescending(r => r.SubmitDate)
                .ToList();
        }

        public List<Reward> GetCurrentRewards()
        {
            return _db.Rewards.Where(r => r.Review.IsApproved == true && (r.ExpireDate == null || r.ExpireDate > DateTime.Now))
                .Include(r => r.Creator)
                .OrderBy(r => r.Name)
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

        public RewardEvent GetRewardEvent(int rewardId, int eventId)
        {
            return _db.RewardEvents.Where(r => r.RewardId == rewardId && r.EventId == eventId).SingleOrDefault();
        }

        public void AddRewardEvent(RewardEvent rewardEvent) => _db.RewardEvents.Add(rewardEvent);

        public void RemoveRewardEvent(RewardEvent rewardEvent) => _db.RewardEvents.Remove(rewardEvent);

        public RewardAttachment GetAttachment(int id) => _db.RewardAttachments.Find(id);

        public bool IsAttachedToReward(int fileId)
        {
            return _db.RewardAttachments.Where(a => a.FileId == fileId).Count() > 0;
        }

        public void SaveChanges() => _db.SaveChanges();
    }
}
