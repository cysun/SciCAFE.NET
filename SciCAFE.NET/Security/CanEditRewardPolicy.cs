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
    public class CanEditRewardRequirement : IAuthorizationRequirement
    {
    }

    // Administrator can edit any reward at any time.
    // RewardProvider can edit their own reward at any time.
    // Regular user can edit their own reward only if the reward has not been submitted for review.
    public class CanEditRewardHandler : AuthorizationHandler<CanEditRewardRequirement, Reward>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CanEditRewardRequirement requirement, Reward reward)
        {
            var isAdministrator = context.User.HasClaim(ClaimType.IsAdministrator, "true");
            var isRewardProvider = context.User.HasClaim(ClaimType.IsRewardProvider, "true");
            var isCreator = context.User.HasClaim(ClaimTypes.NameIdentifier, reward.CreatorId);
            var isSubmitted = reward.SubmitDate != null;

            if (isAdministrator || isRewardProvider && isCreator || isCreator && !isSubmitted)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
