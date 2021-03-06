﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;

namespace SciCAFE.NET.Security.Constants
{
    public static class ClaimType
    {
        public const string IsAdministrator = "IsAdministrator";
        public const string IsEventOrganizer = "IsEventOrganizer";
        public const string IsEventReviewer = "IsEventReviewer";
        public const string IsRewardProvider = "IsRewardProvider";
        public const string IsRewardReviewer = "IsRewardReviewer";
    }

    public static class Policy
    {
        public const string IsAdministrator = "IsAdministrator";
        public const string IsEventReviewer = "IsEventReviewer";
        public const string IsRewardReviewer = "IsRewardReviewer";
        public const string CanEditEvent = "CanEditEvent";
        public const string CanDeleteEvent = "CanDeleteEvent";
        public const string CanReviewEvent = "CanReviewEvent";
        public const string CanManageAttendance = "CanManageAttendance";
        public const string CanEditReward = "CanEditReward";
        public const string CanDeleteReward = "CanDeleteReward";
        public const string CanReviewReward = "CanReviewReward";
        public const string CanAddQualifyingEvent = "CanAddQualifyingEvent";
        public const string CanViewRewardees = "CanViewRewardees";
        public const string CanEmail = "CanEmail";
    }
}
