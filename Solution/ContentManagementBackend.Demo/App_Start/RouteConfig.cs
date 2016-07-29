using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ContentManagementBackend.Demo
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.LowercaseUrls = true;
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Category",
                url: "category/{categoryurl}",
                defaults: new { controller = "Posts", action = "Category" }
            );

            routes.MapRoute(
                name: "Full post",
                url: "post/{id}",
                defaults: new { controller = "Posts", action = "Full" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Posts", action = "List", id = UrlParameter.Optional }
            );
        }
    }
}
