using Common.Utility;
using Common.Utility.Pipelines;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;

namespace ContentManagementBackend.Demo
{
    public class AuthPipelineBase<TInput, TOutput> : Pipeline<TInput, TOutput>
    {
        //поля
        protected ICommonLogger _logger;
        protected MongoUserManager _userManager;
        protected IAuthenticationManager _authenticationManager;


        //инициализация
        public AuthPipelineBase(ICommonLogger logger, MongoUserManager userManager
            , IAuthenticationManager authenticationManager)
        {
            _logger = logger;
            _userManager = userManager;
            _authenticationManager = authenticationManager;
        }


        //методы
        public virtual async Task SignIn(HttpContext httpContext, UserAccount user)
        {
            ClaimsIdentity userIdentity = await _userManager.GenerateIdentityClaims(user);

            _authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            _authenticationManager.SignIn(
                new AuthenticationProperties() { IsPersistent = true }, userIdentity);

            httpContext.User = new ClaimsPrincipal(userIdentity);
        }

        public virtual string CreateAntiForgeryToken(HttpContext httpContext)
        {
            string cookieToken;
            string formToken;

            if (httpContext.Request.Cookies[AntiForgeryConfig.CookieName] == null)
            {
                AntiForgery.GetTokens(null, out cookieToken, out formToken);
                httpContext.Request.Cookies.Add(new HttpCookie(AntiForgeryConfig.CookieName, cookieToken));
            }
            else
            {
                string oldCookieToken = httpContext.Request.Cookies[AntiForgeryConfig.CookieName].Value;
                AntiForgery.GetTokens(oldCookieToken, out cookieToken, out formToken);

                if (cookieToken != null)
                    httpContext.Request.Cookies[AntiForgeryConfig.CookieName].Value = cookieToken;
            }

            return formToken;
        }
    }
}