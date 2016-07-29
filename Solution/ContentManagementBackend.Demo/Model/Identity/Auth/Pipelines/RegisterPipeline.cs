using Common.Identity2_1;
using Common.Utility;
using Common.Utility.Pipelines;
using ContentManagementBackend.Demo.App_Resources;
using ContentManagementBackend.Resources;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ContentManagementBackend.Demo
{
    public class RegisterPipeline : AuthPipelineBase<RegisterPipelineModel, AuthResult>
    {
        //поля
        protected AvatarImageQueries _avatarImageQueries;



        //инициализация
        public RegisterPipeline(ICommonLogger logger, MongoUserManager userManager
            , IAuthenticationManager authenticationManager, AvatarImageQueries avatarImageQueries)
            : base(logger, userManager, authenticationManager)
        {
            _avatarImageQueries = avatarImageQueries;

            RegisterModules();
        }



        //методы
        protected virtual void RegisterModules()
        {
            Register(ValidateInput);
            Register(CheckUniqueEmail);
            Register(CreateUser);
            Register(SendEmail);
        }

        public override async Task<AuthResult> Process(RegisterPipelineModel inputModel)
        {
            var outputModel = AuthResult.Success(null);
            return await Process(inputModel, outputModel);
        }




        //методы
        public virtual Task<bool> ValidateInput(
            PipelineContext<RegisterPipelineModel, AuthResult> context)
        {
            if (string.IsNullOrEmpty(context.Input.Email))
            {
                context.Output = AuthResult.Fail(GlobalContent.CommentAuthPipeline_EmailRequired);
                return Task.FromResult(false);
            }

            bool isValidEmail = true;
            try
            {
                new MailAddress(context.Input.Email);
            }
            catch (FormatException)
            {
                isValidEmail = false;
            }

            if (!isValidEmail)
            {
                context.Output = AuthResult.Fail(GlobalContent.CommentAuthPipeline_EmailBadFormed);
                return Task.FromResult(false);
            }

            if (string.IsNullOrEmpty(context.Input.Name))
            {
                context.Output = AuthResult.Fail(GlobalContent.CommentAuthPipeline_NameRequired);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
        
        public virtual async Task<bool> CheckUniqueEmail(
            PipelineContext<RegisterPipelineModel, AuthResult> context)
        {
            string email = context.Input.Email;
            UserAccount existingEmailAccount;

            try
            {
                existingEmailAccount = await _userManager.FindByEmailAsync(email);
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
                context.Output = AuthResult.Fail(MessageResources.Common_DatabaseException);
                return false;
            }

            if (existingEmailAccount != null)
            {
                string error = string.Format(GlobalContent.CommentAuthPipeline_DuplicateEmail, email);
                context.Output = AuthResult.Fail(error);
                return false;
            }

            return true;
        }

        public virtual async Task<bool> CreateUser(
          PipelineContext<RegisterPipelineModel, AuthResult> context)
        {
            var user = new UserAccount()
            {
                Id = ObjectId.GenerateNewId(),
                Email = context.Input.Email,
                UserName = context.Input.Name,
                Avatar = _avatarImageQueries.GetDefaultAvatar()
            };

            IdentityResult createResult;
            try
            {
                createResult = await _userManager.CreateAsync(user);
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
                context.Output = AuthResult.Fail(MessageResources.Common_DatabaseException);
                return false;
            }

            if (!createResult.Succeeded)
            {
                string error = createResult.Errors.FirstOrDefault();
                context.Output = AuthResult.Fail(error);
                return false;
            }

            context.Output.User = user;
            return true;
        }

        public virtual async Task<bool> SendEmail(
            PipelineContext<RegisterPipelineModel, AuthResult> context)
        {
            UserAccount user = context.Output.User;
            var url = new UrlHelper(context.Input.HttpContext.Request.RequestContext);

            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user.Id);
            string callbackUrl = url.Action("ConfirmEmail", "Account"
                , new { userId = user.Id, code = code }, protocol: context.Input.HttpContext.Request.Url.Scheme);

            string subject = IdentityResourceHelper.Load(IdentitySettings.IdentityResource, "Notify_ConfirmEmail_Subject");
            string bodyFormatString = IdentityResourceHelper.Load(IdentitySettings.IdentityResource, "Notify_ConfirmEmail_Body");
            string body = string.Format(bodyFormatString, callbackUrl);
            await _userManager.SendEmailAsync(user.Id, subject, body);

            return true;
        }
    }
}