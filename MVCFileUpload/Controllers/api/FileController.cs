using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;

namespace MVCFileUpload.Controllers.api
{
    [RoutePrefix("api/fileutils")]
    public class FileController : BaseApiController
    {
        private readonly string msSaveToFolder = Helper.SaveToFolder;

        #region Upload File

        /// <summary>
        /// Handle the Http Post
        /// POST api/fileutils/upload
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("upload")]
        public result UploadFile()
        {
            // validate request
            ValidateRequestHeader();

            var destFileName = string.Empty;
            var tempFolder = string.Empty;
            var uid = HttpContext.Current.Request.Form["uid"];

            if (!string.IsNullOrEmpty(uid))
            {
                if (uid == "init")
                    return Helper.GenerateSuccessHttpResponse(Path.GetFileName("Initialized"));

                tempFolder = Path.Combine(msSaveToFolder, uid);
            }
            if (HttpContext.Current.Request.Files.Count > 0)
            {
                // Get a reference to the file that our jQuery sent.
                var file = HttpContext.Current.Request.Files[0];
                destFileName = Path.Combine(tempFolder, Path.GetFileName(file.FileName));

                if (!Directory.Exists(tempFolder))
                    Directory.CreateDirectory(tempFolder);

                if (!Directory.Exists(msSaveToFolder))
                    Directory.CreateDirectory(msSaveToFolder);

                // save the temporary downloaded file)
                using (var outPutFile = new FileStream(destFileName, FileMode.Append, FileAccess.Write))
                {
                    file.InputStream.CopyTo(outPutFile);
                }
            }
            else
            {
                // clean up the temporary folder
                CleanUpTempFolder(tempFolder);
            }

            return Helper.GenerateSuccessHttpResponse(Path.GetFileName(destFileName));
        }

        /// <summary>
        /// get the max requet length
        /// </summary>
        /// <returns></returns>
        private int getMaxRequestLength()
        {
            var loSection = WebConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;

            if (loSection != null)
                return loSection.MaxRequestLength;

            return 0;
        }

        /// <summary>
        /// Remove temp folder
        /// </summary>
        /// <param name="tmpFolder"></param>
        private void CleanUpTempFolder(string tmpFolder)
        {

            if (!string.IsNullOrEmpty(tmpFolder) && Directory.Exists(tmpFolder))
                Directory.Delete(tmpFolder, true);
        }

        /// <summary>
        /// Check if it is the last chunk of file or not
        /// </summary>
        /// <returns></returns>
        private bool IsLastChunk()
        {
            var retVal = true;
            var contentRange = HttpContext.Current.Request.Headers["Content-Range"]; //"bytes 0-9999/879394"

            // check the content range if its same with the file content - 1
            if (contentRange != null)
            {
                // parse the content range 
                var ranges = contentRange.Split('-')[1].Split('/').Select(long.Parse).ToList();
                retVal = ranges[0] == ranges[1] - 1;
            }

            return retVal;
        }

        /// <summary>
        /// Get the unique file
        /// </summary>
        /// <param name="destFileName"></param>
        private void GetUniqueFile(ref string destFileName)
        {
            var files = Directory.GetFiles(msSaveToFolder,
                string.Format("{0}*{1}", Path.GetFileNameWithoutExtension(destFileName),
                    Path.GetExtension(destFileName)));
            var fileCount = files.Length;

            if (fileCount < 1) return;

            long uniqueNo = 0;

            var uniqueNos = files.Where(r => r.Contains(string.Format("){0}", Path.GetExtension(r))))
                .Select(
                    m =>
                        m.Substring(m.LastIndexOf("(", StringComparison.InvariantCultureIgnoreCase) + 1,
                            m.LastIndexOf(").", StringComparison.InvariantCultureIgnoreCase) -
                            m.LastIndexOf("(", StringComparison.InvariantCultureIgnoreCase) - 1))
                .Where(r => long.TryParse(r, out uniqueNo)).ToList();

            uniqueNo = 0;

            if (uniqueNos.Any())
                uniqueNo = uniqueNos.Select(long.Parse).Max();

            destFileName = Path.Combine(Path.GetDirectoryName(destFileName) ?? string.Empty,
                string.Format("{0} ({2}){1}", Path.GetFileNameWithoutExtension(destFileName),
                    Path.GetExtension(destFileName), uniqueNo + 1));
        }

        #endregion

        #region Get File
        /// <summary>
        /// Handle the Http Get
        /// GET api/fileutils/download
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("download")]
        public string GetFile()
        {
            string lsReturn = "No File";
            var lsFolder = HttpContext.Current.Request["id"];
            var lsFileName = HttpContext.Current.Request["name"];

            if(!string.IsNullOrEmpty(lsFolder) && !string.IsNullOrEmpty(lsFileName))
            {
                var lsDownloadFile = Path.Combine(Path.Combine(msSaveToFolder, lsFolder), lsFileName);

                if(File.Exists(lsDownloadFile))
                {
                    lsReturn = lsFileName;

                    HttpContext.Current.Response.AddHeader("Content-Disposition",
                                                           string.Format("attachment; filename=\"{0}\"", Path.GetFileName(lsDownloadFile)));
                    HttpContext.Current.Response.ContentType = "application/octet-stream";
                    HttpContext.Current.Response.ClearContent();
                    HttpContext.Current.Response.TransmitFile(lsDownloadFile);
                    HttpContext.Current.Response.Flush();
                    HttpContext.Current.Response.End();
                }
            }

            return lsReturn;
        }
        #endregion

        #region Delete File
        /// <summary>
        /// Handle the Http Delete
        /// GET api/fileutils/download
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("delete")]
        public bool DeleteFile()
        {
            // validate request
            ValidateRequestHeader();

            var lsFolder = HttpContext.Current.Request["uid"];

            if(!string.IsNullOrEmpty(lsFolder))
            {
                lsFolder = Path.Combine(msSaveToFolder, lsFolder);
                if(Directory.Exists(lsFolder))
                {
                    Directory.Delete(lsFolder, true);

                    return true;
                }
            }

            return false;
        }

        #endregion
    }

    public class file
    {
        public string uid { get; set; }
        public string name { get; set; }
    }

    public class result
    {
        public result()
        {
            files = new List<file>();
        }
        public IList<file> files { get; set; }
        public string error { get; set; }
    }
}
