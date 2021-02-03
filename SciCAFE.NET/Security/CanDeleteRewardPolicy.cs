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
    public class CanDeleteRewardRequirement : IAuthorizationRequirement
    {
    }

    public class CanDeleteRewardHandler : AuthorizationHandler<CanDeleteRewardRequirement, Reward>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CanDeleteRewardRequirement requirement, Reward reward)
        {
            var isAdministrator = context.User.HasClaim(ClaimType.IsAdministrator, "true");
            var isCreator = context.User.HasClaim(ClaimTypes.NameIdentifier, reward.CreatorId);

            if (isCreator || isAdministrator)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
