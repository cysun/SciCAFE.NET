using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SciCAFE.NET.Models
{
    [Owned]
    public class Review
    {
        public bool? IsApproved { get; set; }

        public string Comments { get; set; }

        public DateTime Timestamp { get; set; }

        public int ReviewerId { get; set; }
        public User Reviewer { get; set; }
    }
}
