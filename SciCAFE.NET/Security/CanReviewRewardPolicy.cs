using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MimeKit;
using SciCAFE.NET.Models;
using SciCAFE.NET.Security.Constants;

namespace SciCAFE.NET.Security
{
    public class CanReviewRewardRequirement : IAuthorizationRequirement
    {
    }

    // Administrator and RewardReviewer can review any reward.
    // RewardProvider can review their own reward.
    public class CanReviewRewardHandler : AuthorizationHandler<CanReviewRewardRequirement, Reward>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CanReviewRewardRequirement requirement, Reward evnt)
        {
            var isAdministrator = context.User.HasClaim(ClaimType.IsAdministrator, "true");
            var isRewardReviewer = context.User.HasClaim(ClaimType.IsRewardReviewer, "true");
            var isRewardProvider = context.User.HasClaim(ClaimType.IsRewardProvider, "true");
            var isCreator = context.User.HasClaim(ClaimTypes.NameIdentifier, evnt.CreatorId);

            if (isAdministrator || isRewardReviewer || isRewardProvider && isCreator)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
