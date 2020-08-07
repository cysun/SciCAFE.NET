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
    public class CanEditEventRequirement : IAuthorizationRequirement
    {
    }

    // Administrator can edit any event at any time.
    // EventOrganizer can edit their own event at any time.
    // Regular user can edit their own event only if the event has not been submitted for review.
    public class CanEditEventHandler : AuthorizationHandler<CanEditEventRequirement, Event>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CanEditEventRequirement requirement, Event evnt)
        {
            var isAdministrator = context.User.HasClaim(ClaimType.IsAdministrator, "true");
            var isEventOrganizer = context.User.HasClaim(ClaimType.IsEventOrganizer, "true");
            var isCreator = context.User.HasClaim(ClaimTypes.NameIdentifier, evnt.CreatorId);
            var isSubmitted = evnt.SubmitDate != null;

            if (isAdministrator || isEventOrganizer && isCreator || isCreator && !isSubmitted)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
