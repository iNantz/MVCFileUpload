using System.Web;
using System.Web.Optimization;

namespace MVCFileUpload
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryfileUpload").Include(
                      "~/Scripts/jQuery.FileUpload/load-image.all.min.js",
                      "~/Scripts/jQuery.FileUpload/canvas-to-blob.min.js",
                      "~/Scripts/jQuery.FileUpload/jquery.iframe-transport.js",
                      "~/Scripts/jQuery.FileUpload/jquery.fileupload.js",
                      "~/Scripts/jQuery.FileUpload/jquery.fileupload-process.js",
                      //"~/Scripts/jQuery.FileUpload/jquery.fileupload-image.js",
                      //"~/Scripts/jQuery.FileUpload/jquery.fileupload-audio.js",
                      //"~/Scripts/jQuery.FileUpload/jquery.fileupload-video.js",
                      //"~/Scripts/jQuery.FileUpload/jquery.fileupload-validate.js",
                      "~/Scripts/jQuery.FileUpload/fileupload.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/jQuery.FileUpload/css/jquery.fileupload.css",
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

        }
    }
}
