using ContentManagementBackend.MongoDbStorage;
using ContentManagementBackend.Demo;
using Common.Utility;
using Common.Web;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ContentManagementBackend.Demo
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //Performance tweaks
            ViewEngines.Engines.Clear();
            IViewEngine RazorEngine = new RazorViewEngine() { FileExtensions = new string[] { "cshtml" } };
            ViewEngines.Engines.Add(RazorEngine);

            //Use ObjectId as Action parameters
            ModelBinders.Binders.Add(typeof(ObjectId), new ObjectIdModelBinder());
            ModelBinders.Binders.Add(typeof(ObjectId?), new ObjectIdModelBinder());
            ModelBinders.Binders.Add(typeof(List<ObjectId>), new ObjectIdModelBinder());
            
            //Init mongodb context
            Common.MongoDb.ObjectIdTypeConverter.Register();
            MongoDbContext.ApplyGlobalSerializationSettings();

            //Other standart stuff
            Startup.ConfigureIdentitySettings();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            Compression.GZipEncodePage();
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            app.Response.Filter = null;

            Exception exception = Server.GetLastError();
            ICommonLogger logger = new NLogLogger();

            if (logger != null)
                logger.Exception(exception, "Unhandled exception");
        }
    }
}
