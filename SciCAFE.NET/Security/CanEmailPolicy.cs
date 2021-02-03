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
    public class CanEmailRequirement : IAuthorizationRequirement
    {
    }

    public class CanEmailAttendeesHandler : AuthorizationHandler<CanEmailRequirement, Event>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CanEmailRequirement requirement, Event evnt)
        {
            var isAdministrator = context.User.HasClaim(ClaimType.IsAdministrator, "true");
            var isCreator = context.User.HasClaim(ClaimTypes.NameIdentifier, evnt.CreatorId);
            var isEventApproved = evnt.Review?.IsApproved == true;

            if (isAdministrator || isCreator && isEventApproved)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class CanEmailRewardeesHandler : AuthorizationHandler<CanEmailRequirement, Reward>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CanEmailRequirement requirement, Reward reward)
        {
            var isAdministrator = context.User.HasClaim(ClaimType.IsAdministrator, "true");
            var isCreator = context.User.HasClaim(ClaimTypes.NameIdentifier, reward.CreatorId);
            var isRewardApproved = reward.Review?.IsApproved == true;

            if (isAdministrator || isCreator && isRewardApproved)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
