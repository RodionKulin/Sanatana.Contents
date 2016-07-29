using System.Globalization;
using Common.Identity2_1;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Reflection;
using System.Threading;
using System.Resources;
using System.Web.Routing;
using System.IO;
using Common.Web;
using System.Security.Principal;
using System.Net;
using ContentManagementBackend.Demo.App_Resources;
using Common.Utility;

namespace ContentManagementBackend.Demo.Controllers
{
       
    public class AccountController : BaseController
    {
        //поля
        private MongoUserManager _userManager;
        private MongoSignInManager _signInManager;
        private IAuthenticationManager _authenticationManager;
        private AuthManager _authManager;
        private ICommonLogger _logger;


        //инициализация
        public AccountController(MongoUserManager userManager
            , IAuthenticationManager authenticationManager, MongoSignInManager signInManager
            , CustomContentManager contentManager, AuthManager authManager
            , ICommonLogger logger)
            : base(contentManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authenticationManager = authenticationManager;
            _authManager = authManager;
            _logger = logger;
        }



        //
        // GET: /Account/Login
        public ActionResult Login(string returnUrl)
        {
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            model.RememberMe = true;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SignInStatus result;
            try
            {
                result = await _signInManager.PasswordEmailSignInAsync(model.Email, model.Password
                    , model.RememberMe, shouldLockout: IdentitySettings.ShouldLockOut);
            }
            catch(Exception exception)
            {
                _logger.Exception(exception);
                result = SignInStatus.Failure;
            }
            
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");               
                case SignInStatus.Failure:
                default:
                    string invalidMessage = IdentityResourceHelper.Load(IdentitySettings.IdentityResource, "AccountController_Login_Invalid");
                    ModelState.AddModelError("", invalidMessage);
                    
                    return View(model);
            }
        }

        [HttpPost]
        public async Task<ActionResult> ULoginCallback()
        {
            string token = Request.Form["token"];
            AuthResult authResult = await _authManager.AuthULogin(token);

            if (!authResult.Result)
            {
                ModelState.AddModelError("", authResult.Message);                
                return View("Login");
            }
            else
            {
                return View("Login");
            }            
        }

        [HttpPost]
        public async Task<ActionResult> ULoginAjaxCallback(string returnUrl)
        {
            string token = Request.Form["token"];
            AuthResult authResult = await _authManager.AuthULogin(token);
            
            if(!authResult.Result)
            {
                return Json(new
                {   
                    error = authResult.Message
                });
            }

            string loginRedirect = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : new UrlHelper(ControllerContext.RequestContext).Action(
                    IdentitySettings.LogInRedirectAction, IdentitySettings.LogInRedirectController);
            
            return Json(new {
                redirect = loginRedirect,
                userName = authResult.User.UserName,
                avatar = _postManager.AvatarImageQueries.PathCreator.CreateStaticUrl(authResult.User.Avatar),
                antiForgeryToken = authResult.AntiForgeryToken
            });
        }
        
        //
        // GET: /Account/Register
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterNameViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            AuthResult result = await _authManager.Register(model);
            if (result.Result)
            {
                return View("DisplayEmail");
            }
            else
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
        }

        //
        // GET: /Account/ConfirmEmail
        public async Task<ActionResult> ConfirmEmail(ObjectId userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }

            IdentityResult result;
            try
            {
                result = await _userManager.ConfirmEmailAsync(userId, code);
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
                return View("Error");
            }
            
            if (result.Succeeded)
            {
                var user = _userManager.FindById(userId);
                
                ClaimsIdentity userIdentity = await _userManager.GenerateIdentityClaims(user);
                _authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                _authenticationManager.SignIn(
                    new AuthenticationProperties() { IsPersistent = true },
                     userIdentity);
                
                HttpContext.User = new ClaimsPrincipal(userIdentity);
                return RedirectToAction(IdentitySettings.LogInRedirectAction, IdentitySettings.LogInRedirectController);
            }

            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                string subject = IdentityResourceHelper.Load(IdentitySettings.IdentityResource, "Notify_ForgotPassword_Subject");
                string bodyFormatString = IdentityResourceHelper.Load(IdentitySettings.IdentityResource, "Notify_ForgotPassword_Body");
                string body = string.Format(bodyFormatString, callbackUrl);
                await _userManager.SendEmailAsync(user.Id, subject, body);

                return View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            IdentityResult result = await _userManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        
        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            _authenticationManager.SignOut();

            if (Url.IsLocalUrl(Request.UrlReferrer.AbsolutePath))
            {
                RouteValueDictionary rvd = WebParsing.ToRouteValueDictionary(Request.UrlReferrer);
                return RedirectToRoute(rvd);
            }
            else
            {
                string action = IdentitySettings.LogOffRedirectAction;
                string controller = IdentitySettings.LogOffRedirectController;
                return RedirectToAction(action, controller);
            }
        }

       

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";
        
        private void AddErrors(IdentityResult result)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(IdentitySettings.LogInRedirectAction
                , IdentitySettings.LogInRedirectController);
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}