using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ContentManagementBackend.Demo.Startup))]
namespace ContentManagementBackend.Demo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            AutofacConfig.Configure(app);
        }
    }
}
