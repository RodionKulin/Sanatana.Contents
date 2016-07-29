using Common.Identity2_1;
using FluentSecurity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ContentManagementBackend.Demo
{
    public class FluentDefaultPolicyViolationHandler : IPolicyViolationHandler
    {
        public ActionResult Handle(PolicyViolationException exception)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();

            routeValueDictionary["action"] = IdentitySettings.LogInPageAction;
            routeValueDictionary["controller"] = IdentitySettings.LogInPageController;
            routeValueDictionary["returnurl"] = HttpContext.Current.Request.RawUrl;
            
            return new RedirectToRouteResult(routeValueDictionary);
        }
    }
}