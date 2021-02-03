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
    public class CanDeleteEventRequirement : IAuthorizationRequirement
    {
    }

    public class CanDeleteEventHandler : AuthorizationHandler<CanDeleteEventRequirement, Event>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CanDeleteEventRequirement requirement, Event evnt)
        {
            var isAdministrator = context.User.HasClaim(ClaimType.IsAdministrator, "true");
            var isCreator = context.User.HasClaim(ClaimTypes.NameIdentifier, evnt.CreatorId);

            if (isCreator || isAdministrator)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
