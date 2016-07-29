using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ContentManagementBackend.Demo.Controllers
{
    public class ErrorController : BaseController
    {


        //инициализация
        public ErrorController(CustomContentManager contentManager)
            : base(contentManager)
        {
        }

        
        // GET: Error
        public ActionResult NoSuchPage()
        {

            HttpContext.Response.StatusCode = 404;
            HttpContext.Response.TrySkipIisCustomErrors = true;

            return View();
    }
        
        public ActionResult Internal()
        {
            HttpContext.Response.StatusCode = 500;
            HttpContext.Response.TrySkipIisCustomErrors = true;

            return View();
        }
    }
}