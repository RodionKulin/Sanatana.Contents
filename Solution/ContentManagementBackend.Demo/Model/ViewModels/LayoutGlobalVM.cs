using ContentManagementBackend;
using IndividualSiteMap.MVC;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContentManagementBackend.Demo
{
    public class LayoutGlobalVM
    {
        //Route
        public string Action { get; set; }
        public string Controller { get; set; }

        //User
        public UserAccount CurrentUser { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsStuffMember
        {
            get
            {
                return HttpContext.Current.User.IsInAnyRole(SiteConstants.STUFF_ROLES);
            }
        }

        //Menu
        public List<Category<ObjectId>> FooterCategories { get; set; }
        public List<RenderItem> PublicMenuItems { get; set; }
        public List<RenderItem> AdminMenuItems { get; set; }
        public RenderItem ActiveMenuItem { get; set; }
        public bool ActiveMenuItemIsParent
        {
            get
            {
                return ActiveMenuItem != null && ActiveMenuItem.Children.Count > 0;
            }
        }

        //Sidebar Content
        public List<ContentRenderVM<ObjectId>> PopularPosts { get; set; }
        public List<CommentRenderVM<ObjectId>> LatestComments { get; set; }
    }
}