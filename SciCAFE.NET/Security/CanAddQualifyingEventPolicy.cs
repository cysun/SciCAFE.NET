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
    public class CanAddQualifyingEventRequirement : IAuthorizationRequirement
    {
    }

    public class CanAddQualifyingEventHandler : AuthorizationHandler<CanAddQualifyingEventRequirement, Reward>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CanAddQualifyingEventRequirement requirement, Reward reward)
        {
            var isAdministrator = context.User.HasClaim(ClaimType.IsAdministrator, "true");
            var isCreator = context.User.HasClaim(ClaimTypes.NameIdentifier, reward.CreatorId);

            if (isAdministrator || isCreator)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
