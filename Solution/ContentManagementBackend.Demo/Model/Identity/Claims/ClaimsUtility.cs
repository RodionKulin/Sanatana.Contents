using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace ContentManagementBackend.Demo
{
    public static class ClaimsUtility
    {
        //методы
        public static List<Claim> GetCurrentUserClaims()
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return new List<Claim>();
            }

            ClaimsPrincipal principal = (ClaimsPrincipal)HttpContext.Current.User;
            if (principal == null)
            {
                return new List<Claim>();
            }

            return principal.Claims.ToList();
        }

        public static string GetCurrentUserAvatar()
        {
            List<Claim> claims = GetCurrentUserClaims();
            
            Claim avatarClaim = claims.FirstOrDefault(p => p.Type == SiteConstants.CLAIM_NAME_USER_AVATAR);

            return avatarClaim == null
                ? null
                : avatarClaim.Value;
        }

        public static List<object> GetCurrentUserRoles()
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return new List<object>();
            }

            ClaimsPrincipal principal = (ClaimsPrincipal)HttpContext.Current.User;
            if (principal == null)
            {
                return new List<object>();
            }

            return GetUserRoles(principal.Claims);
        }

        public static List<object> GetUserRoles(IEnumerable<Claim> claims)
        {
            var roles = new List<object>();

            List<Claim> roleClaims = claims.Where(p => p.Type == ClaimTypes.Role).ToList();

            foreach (Claim item in roleClaims)
            {
                IdentityUserRole role;
                if(Enum.TryParse(item.Value, out role))
                {
                    roles.Add(role);
                }                
            }
            
            return roles;
        }
        
    }
}