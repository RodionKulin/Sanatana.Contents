using Common.Identity2_1;
using Common.Identity2_1.MongoDb;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ContentManagementBackend.Demo.Controllers
{
    [Authorize]
    public class ManageController : BaseController
    {
        //поля
        private MongoUserManager _userManager;

        

        //инициализация
        public ManageController(MongoUserManager userManager, CustomContentManager contentManager)
            : base(contentManager)
        {
            _userManager = userManager;
        }




        //
        // GET: /Account/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? IdentityResourceHelper.Load(IdentitySettings.IdentityResource, "ManageController_Index_ChangePasswordSuccess")
                : message == ManageMessageId.SetPasswordSuccess ? IdentityResourceHelper.Load(IdentitySettings.IdentityResource, "ManageController_Index_SetPasswordSuccess")
                : message == ManageMessageId.SetTwoFactorSuccess ? IdentityResourceHelper.Load(IdentitySettings.IdentityResource, "ManageController_Index_SetTwoFactorSuccess")
                : message == ManageMessageId.Error ? IdentityResourceHelper.Load(IdentitySettings.IdentityResource, "Common_Error")
                : message == ManageMessageId.AddPhoneSuccess ? IdentityResourceHelper.Load(IdentitySettings.IdentityResource, "ManageController_Index_AddPhoneSuccess")
                : message == ManageMessageId.RemovePhoneSuccess ? IdentityResourceHelper.Load(IdentitySettings.IdentityResource, "ManageController_Index_RemovePhoneSuccess")
                : "";
            
            ManageIndexViewModel model = new ManageIndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await _userManager.GetPhoneNumberAsync(User.Identity.GetUserId2<ObjectId>()),
                TwoFactor = await _userManager.GetTwoFactorEnabledAsync(User.Identity.GetUserId2<ObjectId>()),
                Logins = await _userManager.GetLoginsAsync(User.Identity.GetUserId2<ObjectId>()),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(User.Identity.GetUserId())
            };
            return View(model);
        }

        //
        // GET: /Account/RemoveLogin
        public ActionResult RemoveLogin()
        {
            IList<UserLoginInfo> linkedAccounts = _userManager.GetLogins(User.Identity.GetUserId2<ObjectId>());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return View(linkedAccounts);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            IdentityResult result = await _userManager.RemoveLoginAsync(User.Identity.GetUserId2<ObjectId>(),
                new UserLoginInfo(loginProvider, providerKey));

            if (result.Succeeded)
            {
                var user = await _userManager.FindByIdAsync(User.Identity.GetUserId2<ObjectId>());
                if (user != null)
                {
                    await SignInAsync(user, isPersistent: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }

            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Account/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Account/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Generate the token and send it
            string code = await _userManager.GenerateChangePhoneNumberTokenAsync(
                User.Identity.GetUserId2<ObjectId>(), model.Number);

            if (_userManager.SmsService != null)
            {
                string bodyFormat = IdentityResourceHelper.Load(IdentitySettings.IdentityResource, "Notify_AddPhoneNumber_Body");
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = string.Format(bodyFormat, code)
                };
                await _userManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/RememberBrowser
        [HttpPost]
        public ActionResult RememberBrowser()
        {
            ClaimsIdentity rememberBrowserIdentity = AuthenticationManager
                .CreateTwoFactorRememberBrowserIdentity(User.Identity.GetUserId());

            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = true }
                , rememberBrowserIdentity);

            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/ForgetBrowser
        [HttpPost]
        public ActionResult ForgetBrowser()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/EnableTFA
        [HttpPost]
        public async Task<ActionResult> EnableTFA()
        {
            await _userManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId2<ObjectId>(), true);
            var user = await _userManager.FindByIdAsync(User.Identity.GetUserId2<ObjectId>());
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTFA
        [HttpPost]
        public async Task<ActionResult> DisableTFA()
        {
            await _userManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId2<ObjectId>(), false);
            var user = await _userManager.FindByIdAsync(User.Identity.GetUserId2<ObjectId>());
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Account/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            // This code allows you exercise the flow without actually sending codes
            // For production use please register a SMS provider in IdentityConfig and generate a code here.
            string code = await _userManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId2<ObjectId>(), phoneNumber);

            return phoneNumber == null
                ? View("Error")
                : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Account/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ObjectId userId = User.Identity.GetUserId2<ObjectId>();
            IdentityResult result = await _userManager.ChangePhoneNumberAsync(userId
                , model.PhoneNumber, model.Code);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByIdAsync(User.Identity.GetUserId2<ObjectId>());
                if (user != null)
                {
                    await SignInAsync(user, isPersistent: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }

            // If we got this far, something failed, redisplay form
            string errorMessage = IdentityResourceHelper.Load(IdentitySettings.IdentityResource, "ManageController_VerifyPhoneNumber_Error");
            ModelState.AddModelError("", errorMessage);
            return View(model);
        }

        //
        // GET: /Account/RemovePhoneNumber
        public async Task<ActionResult> RemovePhoneNumber()
        {
            IdentityResult result = await _userManager.SetPhoneNumberAsync(User.Identity.GetUserId2<ObjectId>(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            
            var user = await _userManager.FindByIdAsync(User.Identity.GetUserId2<ObjectId>());
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            IdentityResult result = await _userManager.ChangePasswordAsync(User.Identity.GetUserId2<ObjectId>()
                , model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByIdAsync(User.Identity.GetUserId2<ObjectId>());
                if (user != null)
                {
                    await SignInAsync(user, isPersistent: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await _userManager.AddPasswordAsync(
                    User.Identity.GetUserId2<ObjectId>(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByIdAsync(User.Identity.GetUserId2<ObjectId>());
                    if (user != null)
                    {
                        await SignInAsync(user, isPersistent: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Manage
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? IdentityResourceHelper.Load(IdentitySettings.IdentityResource, "ManageController_ManageLogins_RemoveLoginSuccess")
                : message == ManageMessageId.Error ? IdentityResourceHelper.Load(IdentitySettings.IdentityResource, "Common_Error")
                : "";
            var user = await _userManager.FindByIdAsync(User.Identity.GetUserId2<ObjectId>());
            if (user == null)
            {
                return View("Error");
            }
            IList<UserLoginInfo> userLogins = await _userManager.GetLoginsAsync(
                User.Identity.GetUserId2<ObjectId>());
            List<AuthenticationDescription> otherLogins = AuthenticationManager
                .GetExternalAuthenticationTypes()
                .Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider))
                .ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider
                , Url.Action("LinkLoginCallback", "Manage")
                , User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            ExternalLoginInfo loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            IdentityResult result = await _userManager.AddLoginAsync(User.Identity.GetUserId2<ObjectId>(), loginInfo.Login);
            return result.Succeeded
                ? RedirectToAction("ManageLogins")
                : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(UserAccount user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.TwoFactorCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties
            {
                IsPersistent = isPersistent
            }, await _userManager.GenerateIdentityClaims(user));
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = _userManager.FindById(User.Identity.GetUserId2<ObjectId>());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = _userManager.FindById(User.Identity.GetUserId2<ObjectId>());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        #endregion
    }
}