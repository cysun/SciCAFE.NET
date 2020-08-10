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
    public class CanManageAttendanceRequirement : IAuthorizationRequirement
    {
    }

    public class CanManageAttendanceHandler : AuthorizationHandler<CanManageAttendanceRequirement, Event>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CanManageAttendanceRequirement requirement, Event evnt)
        {
            var isAdministrator = context.User.HasClaim(ClaimType.IsAdministrator, "true");
            var isCreator = context.User.HasClaim(ClaimTypes.NameIdentifier, evnt.CreatorId);
            var isPublished = evnt.Review?.IsApproved == true;

            if (isAdministrator || isCreator && isPublished)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
