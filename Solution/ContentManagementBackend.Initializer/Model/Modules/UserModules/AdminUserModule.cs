using ContentManagementBackend.Demo;
using Common.Identity2_1.MongoDb;
using Common.Utility;
using Microsoft.AspNet.Identity;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Common.Initializer;
using ContentManagementBackend.InitializerModules;

namespace ContentManagementBackend.Initializer
{
    public class AdminUserModule : IInitializeModule
    {
        //поля
        protected MongoUserManager _userManager;
        protected AvatarImageQueries _avatarQueries;
        protected MongoUserQueries<UserAccount> _userQueries;
        protected AdminModuleSettings _settings;
        protected ICommonLogger _logger;

        //события
        public event ProgressDelegate ProgressUpdated;



        //инициализация
        public AdminUserModule(ICommonLogger logger, AdminModuleSettings settings
            , MongoUserManager userManager, AvatarImageQueries avatarQueries
            , MongoUserQueries<UserAccount> userQueries)
        {
            _logger = logger;
            _settings = settings;
            _userManager = userManager;
            _avatarQueries = avatarQueries;
            _userQueries = userQueries;
        }


        //методы
        public string IntroduceSelf()
        {
            return string.Format("Create stuff users");
        }

        public async Task Execute()
        {
            foreach (UserEssentials user in _settings.Users)
            {
                var userAccount = new UserAccount()
                {
                    Id = user.ID == ObjectId.Empty ? ObjectId.GenerateNewId() : user.ID,
                    UserName = user.Name,
                    Email = user.Email,
                    Avatar = user.Avatar == null ? _avatarQueries.GetDefaultAvatar() : user.Avatar,
                    EmailConfirmed = true
                };

                //user
                IdentityResult userResult = await _userManager.CreateAsync(userAccount, user.Password);
                if (!userResult.Succeeded)
                {
                    foreach (string error in userResult.Errors)
                    {
                        _logger.Error(error);
                    }

                    return;
                }

                //claims
                if (user.Roles != null)
                {
                    List<Claim> claims = user.Roles
                        .Select(p => new Claim(ClaimTypes.Role, p))
                        .ToList();

                    await _userQueries.AddClaims(userAccount, claims);
                }
            }
            
        }

    }
}
