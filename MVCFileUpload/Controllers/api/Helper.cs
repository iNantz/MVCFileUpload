using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Helpers;


namespace MVCFileUpload.Controllers.api
{
    public static class Helper
    {
        public const string ServerUploadFolder = @"\\{0}\C$\Temp\Uploaded";

        /// <summary>
        ///  Now we need to wire up a response so that the calling script understands what happened
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static result GenerateSuccessHttpResponse(string fileName, string error = null)
        {
            var uid = HttpContext.Current.Request.Form["uid"];
            var loResult = new result() { error = error };

            loResult.files.Add(new file()
            {
                name = fileName,
                uid = uid
            });

            return loResult;
        }

        public static string SaveToFolder
        {
            get
            {
                string lsSaveToFolder = System.Configuration.ConfigurationManager.AppSettings["SaveToFolder"];
                if (string.IsNullOrEmpty(lsSaveToFolder)) lsSaveToFolder = @"C:\Temp\Uploaded";

                if (!Directory.Exists(lsSaveToFolder))
                    Directory.CreateDirectory(lsSaveToFolder);

                return lsSaveToFolder;
            }
        }

        public static string TempExtension
        {
            get
            {
                string lsTempExtension = System.Configuration.ConfigurationManager.AppSettings["TempExtension"];
                if (string.IsNullOrEmpty(lsTempExtension)) lsTempExtension = @".rjmdownload";

                return lsTempExtension;
            }
        }
    }
}