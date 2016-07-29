using Common.Identity2_1;
using Common.Identity2_1.MongoDb;
using Common.Identity2_1.Resources;
using Common.Utility;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.Demo
{
    public class MongoUserManager : UserManager<UserAccount, ObjectId>
    {
        //инициализация
        public MongoUserManager(IUserStore<UserAccount, ObjectId> store)
            : base(store)
        {
        }
        
        public static MongoUserManager Create(IUserStore<UserAccount, ObjectId> store
            , IDataProtectionProvider dataProtectionProvider, ICommonLogger logger)
        {
            var manager = new MongoUserManager(store);

            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<UserAccount, ObjectId>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = false
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = IdentitySettings.PasswordMinimumLength,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(15);
            manager.MaxFailedAccessAttemptsBeforeLockout = 15;
            
            manager.EmailService = new SmtpEmailService(logger);

            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<UserAccount, ObjectId>(
                    dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }

                
        //методы
        public async Task<ClaimsIdentity> GenerateIdentityClaims(UserAccount user)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            ClaimsIdentity userIdentity = await CreateIdentityAsync(user
                , DefaultAuthenticationTypes.ApplicationCookie);

            Claim claim = new Claim(SiteConstants.CLAIM_NAME_USER_AVATAR, user.Avatar);
            userIdentity.AddClaim(claim);
                        
            return userIdentity;
        }

        
    }
}
