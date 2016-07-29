using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace ContentManagementBackend.Demo
{
    public static class ClaimsExtensions
    {
        public static bool IsInAnyRole(this IPrincipal principal, IEnumerable<IdentityUserRole> roles)
        {
            return ClaimsUtility.GetCurrentUserRoles()
                .Any(p => roles.Contains((IdentityUserRole)p));
        }

        public static bool IsInRole(this IPrincipal principal, IdentityUserRole role)
        {
            return ClaimsUtility.GetCurrentUserRoles().Any(p => (IdentityUserRole)p == role);
        }

        public static string GetAvatar(this IPrincipal principal)
        {
            return ClaimsUtility.GetCurrentUserAvatar();
        }
    }
}