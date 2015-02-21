using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;


namespace MVCFileUpload.Controllers.api
{
    /// <summary>
    /// api base controller
    /// </summary>
    public class BaseApiController : ApiController
    {
        public BaseApiController()
        {
        }

        /// <summary>
        /// validate request header
        /// </summary>
        [NonAction]
        public void ValidateRequestHeader()
        {
            var cookieToken = string.Empty;
            var formToken = string.Empty;

            IEnumerable<string> tokenHeaders;
            if (Request.Headers.TryGetValues("RequestVerificationToken", out tokenHeaders))
            {
                var tokens = tokenHeaders.First().Split(':');
                if (tokens.Length == 2)
                {
                    cookieToken = tokens[0].Trim();
                    formToken = tokens[1].Trim();
                }
            }

            AntiForgery.Validate(cookieToken, formToken);
        }
    }
}