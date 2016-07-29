using ContentManagementBackend.Demo.Controllers;
using Common.Identity2_1;
using FluentSecurity;
using MongoDB.Bson;
using System.Web;
using System.Web.Mvc;

namespace ContentManagementBackend.Demo
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            RegisterFluentSecurity();

            filters.Add(new HandleErrorAttribute());
            filters.Add(new HandleSecurityAttribute(), -1); //FluentSecurity Handler
        }

        private static void RegisterFluentSecurity()
        {
            SecurityConfigurator.Configure(configuration =>
            {
                // Let FluentSecurity know how to get the authentication status of the current user
                configuration.GetAuthenticationStatusFrom(() => HttpContext.Current.User.Identity.IsAuthenticated);

                // Let FluentSecurity know how to get the roles for the current user
                configuration.GetRolesFrom(() => ClaimsUtility.GetCurrentUserRoles());

                // Handle security violations
                configuration.DefaultPolicyViolationHandlerIs(() => new FluentDefaultPolicyViolationHandler());
                
                // This is where you set up the policies you want FluentSecurity to enforce
                configuration.For<AccountController>().Ignore();
                configuration.For<AccountController>(x => x.LogOff()).DenyAnonymousAccess();
                configuration.For<AccountController>(x => x.ResetPassword((string)null)).DenyAnonymousAccess();
                configuration.For<AccountController>(x => x.ResetPassword((ResetPasswordViewModel)null)).DenyAnonymousAccess();


                configuration.For<ManageController>().DenyAnonymousAccess();

                configuration.For<PostsController>().RequireAnyRole(IdentityUserRole.Author);
                configuration.For<PostsController>(x => x.Full("")).Ignore();
                configuration.For<PostsController>(x => x.List(1)).Ignore();
                configuration.For<PostsController>(x => x.Category(null, 1)).Ignore();
                configuration.For<PostsController>(x => x.AjaxList("", null)).Ignore();
                
                configuration.For<CommentsController>().Ignore();
                configuration.For<CommentsController>(x => x.UploadCommentImage()).Ignore();
                configuration.For<CommentsController>(x => x.Update(null)).DenyAnonymousAccess();
                configuration.For<CommentsController>(x => x.Delete(null)).DenyAnonymousAccess();

                configuration.For<CategoriesController>().RequireAnyRole(IdentityUserRole.Admin);
                configuration.For<SearchController>().Ignore();
                configuration.For<ErrorController>().Ignore();
            });
        }
    }
}
