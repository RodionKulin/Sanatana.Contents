using ContentManagementBackend.Demo.App_Resources;
using ContentManagementBackend.Demo.Controllers;
using IndividualSiteMap.MVC;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContentManagementBackend.Demo
{
    public class SiteMenu
    {
        public static void RegisterNodes(List<Category<ObjectId>> categories)
        {
            List<Category<ObjectId>> publicCategories = categories.Where(p =>
                p.Permissions == null || p.Permissions.All(a => a.Key != (int)CategoryPermission.View))
                .ToList();

            List<Category<ObjectId>> adminCategories = categories.Where(p =>
                p.Permissions == null || p.Permissions.Any(a => a.Key == (int)CategoryPermission.View))
                .ToList();

            NavigationNode root = PublicMenu(publicCategories);

            List<NavigationNode> adminNodes = AdminMenu(adminCategories);
            root.Children.AddRange(adminNodes);
            
            IndiSiteMap.Settings.Root = root;
        }

        private static NavigationNode PublicMenu(List<Category<ObjectId>> categories)
        {
            var root = new NavigationNode<PostsController>(x => x.List(1))
            {
                Order = 0,
                RenderTargets = new List<object> { RenderTarget.BreadCrumbs }
            };

            List<Category<ObjectId>> rootCategories =
                categories.Where(p => p.ParentCategoryID == null).ToList();
            List<IGrouping<ObjectId?, Category<ObjectId>>> categoryGroups =
                categories.GroupBy(p => p.ParentCategoryID).ToList();

            foreach (Category<ObjectId> category in rootCategories)
            {
                NavigationNode node = ToCategoryNode(category);

                IGrouping<ObjectId?, Category<ObjectId>> childCategories = categoryGroups
                    .FirstOrDefault(p => p.Key == category.CategoryID);
                if (childCategories != null)
                {
                    node.Children = childCategories.Select(p => ToCategoryNode(p)).ToList();
                    node.IsClickable = false;
                }

                root.Children.Add(node);
            }

            return root;
        }

        private static List<NavigationNode> AdminMenu(List<Category<ObjectId>> categories)
        {
            List<NavigationNode> adminNodes = new List<NavigationNode>();

            //posts
            var articlesParent = new NavigationNode()
            {
                Order = 0,
                RenderTargets = new List<object> { RenderTarget.AdminMenu },
                IsClickable = false,
                Title = GlobalContent.Menu_Posts
            };
            adminNodes.Add(articlesParent);

            articlesParent.Children.Add(new NavigationNode<PostsController>(x => x.Add(null, null))
            {
                Order = 0,
                RenderTargets = new List<object> { RenderTarget.AdminMenu },
                Title = GlobalContent.Menu_PostsAdd
            });

            articlesParent.Children.Add(new NavigationNode<PostsController>(x => x.EditList(1, null))
            {
                Order = 0,
                RenderTargets = new List<object> { RenderTarget.AdminMenu },
                Title = GlobalContent.Menu_PostsEditList
            });

            //manuals categories
            foreach (Category<ObjectId> category in categories)
            {
                NavigationNode node = ToCategoryNode(category);
                adminNodes.Add(node);
            }
            
            return adminNodes;
        }

        private static NavigationNode ToCategoryNode(Category<ObjectId> category)
        {
            category.Permissions = category.Permissions ?? new List<KeyValuePair<int, string>>();
            List<object> allowedRoles = category.Permissions
                .Where(p => p.Key == (int)CategoryPermission.View)
                .Select(p => Enum.Parse(typeof(IdentityUserRole), p.Value))
                .Cast<object>().ToList();

            bool isPublic = allowedRoles.Count == 0;

            return new NavigationNode<PostsController>(x => x.Category(null, 1))
            {
                Title = category.Name,
                RouteValues = new Dictionary<string, object>()
                {
                    { "categoryurl", category.Url }
                },
                RenderTargets = isPublic
                    ? new List<object> { RenderTarget.FullMenu }
                    : new List<object> { RenderTarget.AdminMenu },
                AllowedRoles = allowedRoles
            };
        }
    }
}