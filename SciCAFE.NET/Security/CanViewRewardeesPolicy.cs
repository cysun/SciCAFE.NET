using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SciCAFE.NET.Models;
using SciCAFE.NET.Security.Constants;

namespace SciCAFE.NET.Security
{
    public class CanViewRewardeesRequirement : IAuthorizationRequirement
    {
    }

    // Administrator can view rewardees at any time.
    // Creator can view rewardees only if the reward is approved (i.e. reviewed); otherwise
    // anybody would be able to see event attendance information.
    public class CanViewRewardeesHandler : AuthorizationHandler<CanViewRewardeesRequirement, Reward>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CanViewRewardeesRequirement requirement, Reward reward)
        {
            var isAdministrator = context.User.HasClaim(ClaimType.IsAdministrator, "true");
            var isCreator = context.User.HasClaim(ClaimTypes.NameIdentifier, reward.CreatorId);
            var isApproved = reward.Review?.IsApproved == true;

            if (isAdministrator || isCreator && isApproved)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
