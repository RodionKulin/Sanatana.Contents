using Common.Identity2_1.MongoDb;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Data.Entity.SqlServer.Utilities;

namespace ContentManagementBackend.Demo
{
  
    public class MongoSignInManager : SignInManager<UserAccount, ObjectId>
    {
        //инициализация
        public MongoSignInManager(MongoUserManager userManager, IAuthenticationManager authenticationManager):
            base(userManager, authenticationManager)
        {

        }

        public static MongoSignInManager Create(
            IdentityFactoryOptions<MongoSignInManager> options, IOwinContext context)
        {
            return new MongoSignInManager(context.GetUserManager<MongoUserManager>(), context.Authentication);
        }


        
        //методы
        public override Task<ClaimsIdentity> CreateUserIdentityAsync(UserAccount user)
        {
            return ((MongoUserManager)UserManager).GenerateIdentityClaims(user);
        }

        public async virtual Task<SignInStatus> PasswordEmailSignInAsync(
            string email, string password, bool isPersistent, bool shouldLockout)
        {
            UserAccount user = await UserManager.FindByEmailAsync(email).WithCurrentCulture<UserAccount>();
            if (user == null)
            {
                return SignInStatus.Failure;
            }

            bool introduced24 = await UserManager.IsLockedOutAsync(user.Id).WithCurrentCulture<bool>();
            if (introduced24)
            {
                return SignInStatus.LockedOut;
            }

            bool introduced25 = await UserManager.CheckPasswordAsync(user, password).WithCurrentCulture<bool>();
            if (introduced25)
            {
                return await SignInOrTwoFactor(user, isPersistent).WithCurrentCulture<SignInStatus>();
            }

            if (shouldLockout)
            {
                await UserManager.AccessFailedAsync(user.Id).WithCurrentCulture<IdentityResult>();
                bool introduced27 = await UserManager.IsLockedOutAsync(user.Id).WithCurrentCulture<bool>();
                if (introduced27)
                {
                    return SignInStatus.LockedOut;
                }
            }

            return SignInStatus.Failure;
        }

        private async Task<SignInStatus> SignInOrTwoFactor(UserAccount user, bool isPersistent)
        {
            string userId = Convert.ToString(user.Id);
            bool introduced18 = await UserManager.GetTwoFactorEnabledAsync(user.Id).WithCurrentCulture<bool>();

            if (introduced18)
            {
                IList<string> introduced19 = await UserManager.GetValidTwoFactorProvidersAsync(user.Id).WithCurrentCulture<IList<string>>();
                if (introduced19.Count > 0)
                {
                    bool introduced20 = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId).WithCurrentCulture<bool>();
                    if (!introduced20)
                    {
                        ClaimsIdentity identity = new ClaimsIdentity("TwoFactorCookie");
                        identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId));
                        AuthenticationManager.SignIn(new ClaimsIdentity[] { identity });
                        return SignInStatus.RequiresVerification;
                    }
                }
            }

            await SignInAsync(user, isPersistent, false).WithCurrentCulture();
            return SignInStatus.Success;
        }

    }
}
