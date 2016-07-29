using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ContentManagementBackend.Demo
{
    public static class ViewbagExtensions
    {
        public static LayoutGlobalVM LayoutVM(this HtmlHelper helper)
        {
            return (LayoutGlobalVM)helper.ViewBag.LayoutVM;
        }
    }
}