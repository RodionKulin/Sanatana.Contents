using ContentManagementBackend.Demo.App_Resources;
using Common.Utility;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Common.Utility.Pipelines;
using ContentManagementBackend.Resources;

namespace ContentManagementBackend.Demo
{
    public class ULoginAuthPipeline : AuthPipelineBase<ULoginAuthPipelineModel, AuthResult>
    {  
        //поля
        private AvatarImageQueries _avatarImageQueries;
        private ULoginUser _uLoginUser;
        private UserAccount _userAccount;

        

        //инициализация
        public ULoginAuthPipeline(ICommonLogger logger, MongoUserManager userManager
            , IAuthenticationManager authenticationManager, AvatarImageQueries avatarImageQueries)
            : base (logger, userManager, authenticationManager)
        {
            _avatarImageQueries = avatarImageQueries;

            RegisterModules();
        }

        
        //методы
        protected virtual void RegisterModules()
        {
            Register(RequestULoginUser);
            Register(FindUser);
            Register(CreateUser);
            Register(SignIn);
            Register(CreateAntiForgeryToken);
        }

        public override async Task<AuthResult> Process(ULoginAuthPipelineModel inputModel)
        {
            var outputModel = AuthResult.Success(null);
            return await Process(inputModel, outputModel);
        }



        //этапы
        public virtual async Task<bool> RequestULoginUser(
           PipelineContext<ULoginAuthPipelineModel, AuthResult> context)
        {
            try
            {
                string requestAddress = string.Format(
                    SiteConstants.URL_ULOGIN_REQUEST, context.Input.Token, context.Input.Host);
                Uri requestUri = new Uri(requestAddress);
                string response;

                using (WebClient wc = new WebClient())
                {
                    response = await wc.DownloadStringTaskAsync(requestUri);
                }

                _uLoginUser = Newtonsoft.Json.JsonConvert.DeserializeObject<ULoginUser>(response);
            }
            catch (Exception e)
            {
                _logger.Exception(e);
            }
            
            if (_uLoginUser == null || _uLoginUser.Error != null)
            {
                context.Output = AuthResult.Fail(GlobalContent.Common_AuthError);
                return false;
            }

            return true;
        }

        public virtual async Task<bool> FindUser(
           PipelineContext<ULoginAuthPipelineModel, AuthResult> context)
        {
            UserLoginInfo userLogin = new UserLoginInfo(_uLoginUser.Network, _uLoginUser.Uid);

            try
            {
                _userAccount = await _userManager.FindAsync(userLogin);
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
                context.Output = AuthResult.Fail(MessageResources.Common_DatabaseException);
                return false;
            }            
            
            return true;
        }

        public virtual async Task<bool> CreateUser(
           PipelineContext<ULoginAuthPipelineModel, AuthResult> context)
        {
            if (_userAccount == null)
            {
                string nickName = _uLoginUser.NickName == null
                    ? null
                    : " " + _uLoginUser.NickName;
                
                _userAccount = new UserAccount()
                {
                    Id = ObjectId.GenerateNewId(),
                    UserName = string.Format("{0}{1} {2}", _uLoginUser.First_Name, nickName, _uLoginUser.Last_Name),
                    Avatar = await CreateAvatar(_uLoginUser)
                };


                IdentityResult identityResult;
                try
                {
                    identityResult = await _userManager.CreateAsync(_userAccount);
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                    context.Output = AuthResult.Fail(MessageResources.Common_DatabaseException);
                    return false;
                }
                
                if (!identityResult.Succeeded)
                {
                    context.Output = AuthResult.Fail(identityResult.Errors.FirstOrDefault());
                    return false;
                }

                UserLoginInfo userLogin = new UserLoginInfo(_uLoginUser.Network, _uLoginUser.Uid);
                IdentityResult identityLoginResult;
                try
                {
                    identityLoginResult = await _userManager.AddLoginAsync(_userAccount.Id, userLogin);
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                    context.Output = AuthResult.Fail(MessageResources.Common_DatabaseException);
                    return false;
                }

                if (!identityLoginResult.Succeeded)                    
                {
                    context.Output = AuthResult.Fail(identityLoginResult.Errors.FirstOrDefault());
                    return false;
                }
            }

            context.Output.User = _userAccount;
            return true;
        }

        public virtual async Task<string> CreateAvatar(ULoginUser uLoginUser)
        {
            string url = uLoginUser.Photo_big == null
                ? uLoginUser.Photo
                : uLoginUser.Photo_big;

            if (url == null)
            {
                return _avatarImageQueries.GetDefaultAvatar();
            }

            string avatarName = ShortGuid.NewGuid().ToString();
            PipelineResult<List<ImagePipelineResult>> createResult =
                await _avatarImageQueries.CreateStaticImage(url, avatarName);

            if (!createResult.Result)
            {
                _logger.Error(InnerMessages.ULoginConnector_AvatarCreateError, url, createResult.Message);
                return _avatarImageQueries.GetDefaultAvatar();
            }

            return avatarName;
        }

        public virtual async Task<bool> SignIn(
           PipelineContext<ULoginAuthPipelineModel, AuthResult> context)
        {
            try
            {
                await SignIn(context.Input.HttpContext, _userAccount);
                return true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
                context.Output = AuthResult.Fail(MessageResources.Common_DatabaseException);
                return false;
            }
        }

        public virtual Task<bool> CreateAntiForgeryToken(
            PipelineContext<ULoginAuthPipelineModel, AuthResult> context)
        {
            context.Output.AntiForgeryToken = CreateAntiForgeryToken(context.Input.HttpContext);
            return Task.FromResult(true);
        }

    }
}