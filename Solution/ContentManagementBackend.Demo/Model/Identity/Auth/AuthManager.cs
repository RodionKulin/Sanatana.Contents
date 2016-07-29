using Common.Identity2_1;
using Common.Utility;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;

namespace ContentManagementBackend.Demo
{
    public class AuthManager
    {
        //свойства
        public IIdentityQueries IdentityQueries { get; set; }
        public CommentAuthPipeline CommentAuthPipeline { get; set; }
        public ULoginAuthPipeline ULoginAuthPipeline { get; set; }
        public RegisterPipeline RegisterPipeline { get; set; }


        //инициализация
        public AuthManager(IIdentityQueries identityQueries, IAuthenticationManager authenticationManager
            , MongoUserManager userManager, AvatarImageQueries avatarQueries, ICommonLogger logger)
        {
            IdentityQueries = identityQueries;
            CommentAuthPipeline = new CommentAuthPipeline(logger, userManager, authenticationManager, avatarQueries);
            ULoginAuthPipeline = new ULoginAuthPipeline(logger, userManager, authenticationManager,  avatarQueries);
            RegisterPipeline = new RegisterPipeline(logger, userManager, authenticationManager, avatarQueries);
        }


        //методы
        public Task<AuthResult> AuthComment(CommentAuthPipelineModel authVM)
        {
            authVM.HttpContext = HttpContext.Current;
            return CommentAuthPipeline.Process(authVM);
        }

        public Task<AuthResult> AuthULogin(string token)
        {
            return ULoginAuthPipeline.Process(new ULoginAuthPipelineModel()
            {
                Token = token,
                Host = HttpContext.Current.Request.ServerVariables["SERVER_NAME"],
                HttpContext = HttpContext.Current
            });
        }

        public Task<AuthResult> Register(RegisterNameViewModel model)
        {
            return RegisterPipeline.Process(new RegisterPipelineModel()
            {
                Name = model.UserName,
                Email = model.Email,
                Password = model.Password,
                HttpContext = HttpContext.Current
            });
        }
    }
}