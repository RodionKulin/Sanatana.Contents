using Common.Web;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Optimization;

namespace ContentManagementBackend.Demo
{    

    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            string cdnPath = "d2ffx366x3htfx.cloudfront.net";
            bundles.UseCdn = false;
            //BundleTable.EnableOptimizations = false;


            //Script
            bundles.Add(new CdnScriptBundle("~/content/jsjqueryval", cdnPath).Include(
                        "~/content/scripts/jquery/jquery.validate*"));
            
            bundles.Add(new CdnScriptBundle("~/content/jscore", cdnPath).Include(
                        "~/content/scripts/jquery/jquery-{version}.js",
                        "~/content/scripts/bootstrap/bootstrap.js",
                        "~/content/scripts/plugins/moment/moment-with-ru.js",
                        "~/content/scripts/core/main.js"
                        ));

            bundles.Add(new CdnScriptBundle("~/content/jsajax", cdnPath).Include(
                        "~/content/scripts/plugins/toastr.min.js",
                        "~/content/scripts/plugins/url.min.js",
                        "~/content/scripts/plugins/knockout-3.3.0.js",
                        "~/content/scripts/core/main-ajax.js"
                        ));

            bundles.Add(new CdnScriptBundle("~/content/jsdatetimepicker", cdnPath).Include(
                        "~/content/scripts/plugins/bootstrap-datetimepicker.js"
                        ));

            bundles.Add(new CdnScriptBundle("~/content/jsarticles-list", cdnPath).Include(
                        "~/content/scripts/core/posts-list.js"
                        ));

            bundles.Add(new CdnScriptBundle("~/content/jssearch-list", cdnPath).Include(
                        "~/content/scripts/core/search-list.js"
                        ));

            bundles.Add(new CdnScriptBundle("~/content/jsarticles-edit", cdnPath).Include(
                        "~/content/scripts/plugins/fileupload/jquery.iframe-transport.js",
                        "~/content/scripts/plugins/fileupload/jquery.ui.widget.js",
                        "~/content/scripts/plugins/fileupload/jquery.fileupload.js",
                        "~/content/scripts/bootstrap/bootstrap-confirmation.bs3.js",
                        "~/content/scripts/core/posts-edit.js"
                        ));

            bundles.Add(new CdnScriptBundle("~/content/jsarticles-full", cdnPath).Include(
                        "~/content/scripts/plugins/jquery.prettyPhoto.js",
                        "~/content/scripts/core/comments-list.js",
                        "~/content/scripts/core/posts-full.js"
                        ));


            //Style
            bundles.Add(new CdnStyleBundle("~/content/layout", cdnPath).Include(
                      "~/content/Styles/core/reset.css",
                      "~/content/Styles/bootstrap/bootstrap.css",
                      "~/content/Styles/core/layout.css"));

            bundles.Add(new CdnStyleBundle("~/content/css", cdnPath).Include(
                      "~/content/Styles/plugins/font-awesome.css",
                      "~/content/Styles/plugins/toastr.css"));

            bundles.Add(new CdnStyleBundle("~/content/datetimepicker", cdnPath).Include(
                      "~/content/Styles/plugins/bootstrap-datetimepicker.css"));

            bundles.Add(new CdnStyleBundle("~/content/posts-edit", cdnPath).Include(
                      "~/content/Styles/core/posts-edit.css",
                      "~/content/Styles/core/posts-full.css"));

            bundles.Add(new CdnStyleBundle("~/content/posts-list", cdnPath).Include(
                      "~/content/Styles/plugins/mvcpaging/paging.css",
                      "~/content/Styles/core/posts-list.css"));

            bundles.Add(new CdnStyleBundle("~/content/search-list", cdnPath).Include(
                      "~/content/Styles/core/search-list.css"));

            bundles.Add(new CdnStyleBundle("~/content/posts-full", cdnPath).Include(
                      "~/content/Styles/plugins/prettyPhoto.mod.css",
                      "~/content/Styles/core/posts-full.css",
                      "~/content/Styles/core/comments-list.css"));

        }
    }
}
