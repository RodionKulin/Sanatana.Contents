using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using System.Globalization;
using System.Threading;
using MongoDB.Bson;
using Common.Identity2_1;
using Common.Identity2_1.MongoDb;
using System.Threading.Tasks;
using System.Security.Claims;
using Common.Utility;
using Common.MongoDb;

namespace ContentManagementBackend.Demo
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                //SlidingExpiration = true,
                //ExpireTimeSpan = System.TimeSpan.FromDays(30),
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                ExpireTimeSpan = TimeSpan.FromDays(365 * 10),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<MongoUserManager, UserAccount, ObjectId>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentityCallback: (manager, user) => manager.GenerateIdentityClaims(user),
                        getUserIdCallback: (identity) => identity.GetUserId2<ObjectId>())                        
                }
            });     
            
        }

        public static void ConfigureIdentitySettings()
        {
            IdentitySettings.IdentityResourceCulture = new CultureInfo("ru-RU");
            IdentitySettings.AuthorizationType = AuthorizationType.OnlyPassword;
            IdentitySettings.ShouldLockOut = true;

            //страница входа
            IdentitySettings.LogInPageController = "account";
            IdentitySettings.LogInPageAction = "login";

            //переадресация после входа
            IdentitySettings.LogInRedirectAction = "";
            IdentitySettings.LogInRedirectController = "";

            //переадресация после выхода, если не указана другая страница
            IdentitySettings.LogOffRedirectController = "";
            IdentitySettings.LogOffRedirectAction = "";
        }
    }

}