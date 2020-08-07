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
    public class CanReviewEventRequirement : IAuthorizationRequirement
    {
    }

    // Administrator and EventReviewer can review any event.
    // EventOrganizer can review their own event.
    public class CanReviewEventHandler : AuthorizationHandler<CanReviewEventRequirement, Event>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CanReviewEventRequirement requirement, Event evnt)
        {
            var isAdministrator = context.User.HasClaim(ClaimType.IsAdministrator, "true");
            var isEventReviewer = context.User.HasClaim(ClaimType.IsEventReviewer, "true");
            var isEventOrganizer = context.User.HasClaim(ClaimType.IsEventOrganizer, "true");
            var isCreator = context.User.HasClaim(ClaimTypes.NameIdentifier, evnt.CreatorId);

            if (isAdministrator || isEventReviewer || isEventOrganizer && isCreator)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
