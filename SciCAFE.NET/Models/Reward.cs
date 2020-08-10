using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SciCAFE.NET.Models
{
    public class Reward
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        public int NumOfEventsToQualify { get; set; } = 1;

        public List<RewardEvent> RewardEvents { get; set; } = new List<RewardEvent>();

        public string CreatorId { get; set; }
        public User Creator { get; set; }

        public DateTime? SubmitDate { get; set; }
        public DateTime? ExpireDate { get; set; }

        public Review Review { get; set; }

        public bool IsDeleted { get; set; }
    }

    [Table("RewardEvents")]
    public class RewardEvent
    {
        public int RewardId { get; set; }
        public Reward Reward { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }
    }
}
