using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SciCAFE.NET.Models;
using SciCAFE.NET.Security.Constants;

namespace SciCAFE.NET.Security
{
    public class SecurityUtils
    {
        public static List<Claim> GetAdditionalClaims(User user)
        {
            var claims = new List<Claim>();

            if (user.IsAdministrator)
                claims.Add(new Claim(ClaimType.IsAdministrator, "True"));
            if (user.IsEventOrganizer)
                claims.Add(new Claim(ClaimType.IsEventOrganizer, "True"));
            if (user.IsEventReviewer)
                claims.Add(new Claim(ClaimType.IsEventReviewer, "True"));
            if (user.IsRewardProvider)
                claims.Add(new Claim(ClaimType.IsRewardProvider, "True"));
            if (user.IsRewardReviewer)
                claims.Add(new Claim(ClaimType.IsRewardReviewer, "True"));

            return claims;
        }
    }
}
