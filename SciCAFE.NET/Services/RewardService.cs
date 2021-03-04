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

        public void AddReward(Reward reward) => _db.Rewards.Add(reward);

        public void DeleteReward(Reward reward) => _db.Rewards.Remove(reward);

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

        public List<RewardeeViewModel> GetRewardees(int id)
        {
            var reward = _db.Rewards.Where(r => r.Id == id)
                            .Include(r => r.RewardEvents)
                            .SingleOrDefault();
            var eventIds = reward.RewardEvents.Select(e => e.EventId).ToHashSet();
            var attendances = _db.Attendances.Where(a => eventIds.Contains(a.EventId))
                .Include(a => a.Attendee)
                .OrderBy(a => a.AttendeeId)
                .ToList();

            var rewardees = new List<RewardeeViewModel>();
            var rewardee = new RewardeeViewModel();
            foreach (var attendance in attendances)
            {
                if (attendance.AttendeeId != rewardee.Id)
                {
                    rewardee = new RewardeeViewModel
                    {
                        Id = attendance.Attendee.Id,
                        FirstName = attendance.Attendee.FirstName,
                        LastName = attendance.Attendee.LastName,
                        Email = attendance.Attendee.Email,
                        NumOfEventsToQualify = reward.NumOfEventsToQualify
                    };
                    rewardee.AttendedEventIds.Add(attendance.EventId);
                    rewardees.Add(rewardee);
                }
                else
                {
                    rewardee.AttendedEventIds.Add(attendance.EventId);
                }
            }

            return rewardees;
        }

        public int GetRewardsProvidedCount(string userId)
        {
            return _db.Rewards.Where(r => r.CreatorId == userId).Count();
        }

        public (int, int) GetRewardsQualifiedCounts(string userId)
        {
            var rewardsCount = _db.Rewards.FromSqlInterpolated($@"
                SELECT r.* from ""Rewards"" r
                    INNER JOIN ""RewardEvents"" e on r.""Id"" = e.""RewardId""
                    INNER JOIN ""Attendances"" a on e.""EventId"" = a.""EventId""
                    WHERE a.""AttendeeId"" = {userId}").Distinct().Count();

            var qualifiedRewardsCount = _db.Rewards.FromSqlInterpolated($@"
                SELECT r.* from ""Rewards"" r
                    INNER JOIN ""RewardEvents"" e on r.""Id"" = e.""RewardId""
                    INNER JOIN ""Attendances"" a on e.""EventId"" = a.""EventId""
                    WHERE a.""AttendeeId"" = {userId}
                    GROUP BY r.""Id""
                    HAVING COUNT(e.""EventId"") >= r.""NumOfEventsToQualify""").Count();

            return (qualifiedRewardsCount, rewardsCount);
        }

        public List<Reward> GetRewardsProvided(string userId)
        {
            return _db.Rewards.Where(e => e.CreatorId == userId).OrderByDescending(r => r.Id).ToList();
        }

        public List<Reward> GetRewardsQualified(string userId)
        {
            return _db.Rewards.FromSqlInterpolated($@"
                SELECT r.* from ""Rewards"" r
                    INNER JOIN ""RewardEvents"" e on r.""Id"" = e.""RewardId""
                    INNER JOIN ""Attendances"" a on e.""EventId"" = a.""EventId""
                    WHERE a.""AttendeeId"" = {userId}")
                .Include(r => r.RewardEvents).ThenInclude(e => e.Event)
                .ToList();
        }

        public void SaveChanges() => _db.SaveChanges();
    }
}
