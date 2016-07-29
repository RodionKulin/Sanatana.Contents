using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Web.Routing;
using System.Security.Claims;
using Common.Identity2_1;
using IndividualSiteMap.MVC;
using System.Net.Http;
using System.Diagnostics;
using Common.Utility;
using System.Threading.Tasks;

namespace ContentManagementBackend.Demo.Controllers
{
    public class BaseController : Controller
    {
        //поля
        protected CustomContentManager _postManager;
        protected UserAccount _currentUser;
        protected LayoutGlobalVM _layoutVM;
        protected List<IdentityUserRole> _userRoles;



        //инициализация
        public BaseController(CustomContentManager contentManager)
        {
            _postManager = contentManager;
        }
        
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            if (ControllerContext.IsChildAction)
            {
                return;
            }

            //user
            _userRoles = new List<IdentityUserRole>();
            ReadClaims(requestContext);
            
            //menu            
            List<Category<ObjectId>> categories =
                _postManager.CategoryQueries.Select().Result.Result;
           
            SiteMenu.RegisterNodes(categories);
            List<RenderItem> publicMenuItems = IndiSiteMap.Current.GetMenu(RenderTarget.FullMenu);
            List<RenderItem> adminMenuItems = IndiSiteMap.Current.GetMenu(RenderTarget.AdminMenu);

            //vm
            _layoutVM = new LayoutGlobalVM()
            {
                Controller = requestContext.RouteData.Values["controller"].ToString().ToLower(),
                Action = requestContext.RouteData.Values["action"].ToString().ToLower(),
                CurrentUser = _currentUser,
                AvatarUrl = _currentUser == null
                    ? null
                    : _postManager.AvatarImageQueries.PathCreator.CreateStaticUrl(_currentUser.Avatar),
                FooterCategories = _postManager.CategoryManager.SelectIncluded((int)CategoryPermission.View, new List<string>()).Result.Result,
                PublicMenuItems = publicMenuItems,
                AdminMenuItems = adminMenuItems,
                ActiveMenuItem = publicMenuItems.FirstOrDefault(p => p.IsChildOrSelfCurrentItem)
            };
            ViewBag.LayoutVM = _layoutVM;
        }


        //методы
        private void ReadClaims(RequestContext requestContext)
        {
            if (!requestContext.HttpContext.User.Identity.IsAuthenticated)
            {
                return;
            }

            _currentUser = new UserAccount
            {
                Id = requestContext.HttpContext.User.Identity.GetUserId2<ObjectId>(),
                UserName = requestContext.HttpContext.User.Identity.Name,
                Avatar = requestContext.HttpContext.User.GetAvatar()
            };

            _userRoles = ClaimsUtility.GetCurrentUserRoles().Cast<IdentityUserRole>().ToList();
        }

        protected async Task<bool> SelectSidebarVMData()
        {
            Task<QueryResult<List<ContentRenderVM<ObjectId>>>> popularTask = _postManager.SelectPopular(
               SiteConstants.POSTS_POPULAR_PERIOD, SiteConstants.POSTS_POPULAR_COUNT
               , (int)CategoryPermission.View, SiteConstants.USE_ALL_POSTS_TO_MATCH_POPULAR_COUNT);
            Task<QueryResult<List<CommentRenderVM<ObjectId>>>> commentsTask =
                _postManager.CommentManager.SelectLatest(SiteConstants.LATEST_COMMENTS_COUNT, true);

            QueryResult<List<ContentRenderVM<ObjectId>>> popularPosts = await popularTask;
            QueryResult<List<CommentRenderVM<ObjectId>>> comments = await commentsTask;

            if(popularPosts.HasExceptions || comments.HasExceptions)
            {
                return false;
            }

            foreach (ContentRenderVM<ObjectId> item in popularPosts.Result)
            {
                item.Url = SetFullUrl(SiteConstants.URL_BASE_FULL_POST, item.Content.Url);
            }

            _layoutVM.PopularPosts = popularPosts.Result;
            _layoutVM.LatestComments = comments.Result;
            return true;
        }

        protected string SetFullUrl(string staticStart, string dynamicEnd)
        {
            return dynamicEnd.StartsWith(staticStart)
                ? dynamicEnd
                : staticStart + dynamicEnd;
        }

    }
}