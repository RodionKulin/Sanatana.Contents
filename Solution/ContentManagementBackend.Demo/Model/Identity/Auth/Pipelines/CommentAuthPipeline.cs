using Common.Utility;
using Common.Utility.Pipelines;
using ContentManagementBackend.Demo.App_Resources;
using ContentManagementBackend.Resources;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;

namespace ContentManagementBackend.Demo
{
    public class CommentAuthPipeline : AuthPipelineBase<CommentAuthPipelineModel, AuthResult>
    {
        //поля
        protected AvatarImageQueries _avatarImageQueries;
        protected UserAccount _userAccount;


        //инициализация
        public CommentAuthPipeline(ICommonLogger logger, MongoUserManager userManager
            , IAuthenticationManager authenticationManager, AvatarImageQueries avatarImageQueries)
            :base(logger, userManager, authenticationManager)
        {
            _avatarImageQueries = avatarImageQueries;

            RegisterModules();
        }

        

        //методы
        protected virtual void RegisterModules()
        {
            Register(ValidateInput);
            Register(FindUser);
            Register(CheckUniqueEmail);            
            Register(CreateUser);
            Register(SignIn);
            Register(CreateAntiForgeryToken);
        }

        public override async Task<AuthResult> Process(CommentAuthPipelineModel inputModel)
        {
            var outputModel = AuthResult.Success(null);
            return await Process(inputModel, outputModel);
        }



        //этапы
        public virtual Task<bool> ValidateInput(
            PipelineContext<CommentAuthPipelineModel, AuthResult> context)
        {
            if(string.IsNullOrEmpty(context.Input.Email))
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

            if(!isValidEmail)
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

        public virtual async Task<bool> FindUser(
            PipelineContext<CommentAuthPipelineModel, AuthResult> context)
        {
            try
            {
                _userAccount = await _userManager.FindByEmailAsync(context.Input.Email);
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
                context.Output = AuthResult.Fail(MessageResources.Common_DatabaseException);
                return false;
            }

            
            if(_userAccount == null)
            {
                return true;
            }

            if (_userAccount.PasswordHash != null)
            {
                context.Output = AuthResult.Fail(GlobalContent.CommentAuthPipeline_PasswordRequired);
                context.Output.IsPasswordRequired = true;
                return false;
            }
            
            return true;
        }
        
        public virtual async Task<bool> CheckUniqueEmail(
          PipelineContext<CommentAuthPipelineModel, AuthResult> context)
        {
            if (_userAccount == null)
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
            }

            return true;
        }

        public virtual async Task<bool> CreateUser(
          PipelineContext<CommentAuthPipelineModel, AuthResult> context)
        {
            if (_userAccount == null)
            {
                _userAccount = new UserAccount()
                {
                    Id = ObjectId.GenerateNewId(),
                    Email = context.Input.Email,
                    UserName = context.Input.Name,
                    Avatar = _avatarImageQueries.GetDefaultAvatar()
                };

                IdentityResult createResult;
                try
                {
                    createResult = await _userManager.CreateAsync(_userAccount);
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
            }

            context.Output.User = _userAccount;
            return true;
        }

        public virtual async Task<bool> SignIn(
            PipelineContext<CommentAuthPipelineModel, AuthResult> context)
        {
            try
            {
                await SignIn(context.Input.HttpContext, _userAccount);
                return true;
            }
            catch(Exception exception)
            {
                _logger.Exception(exception);
                context.Output = AuthResult.Fail(MessageResources.Common_DatabaseException);
                return false;
            }
        }
        
        public virtual Task<bool> CreateAntiForgeryToken(
            PipelineContext<CommentAuthPipelineModel, AuthResult> context)
        {
            context.Output.AntiForgeryToken = CreateAntiForgeryToken(context.Input.HttpContext);
            return Task.FromResult(true);
        }
    }
}