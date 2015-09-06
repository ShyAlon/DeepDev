using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using NgFlowSample.Models;
using NgFlowSample.Services;
using UIBuildIt.Models;
using System.Drawing;
using System.IO.Compression;
using System.Text;

namespace NgFlowSample.Controllers
{
    public class FileGetController : ApiController
    {
        private readonly string path =  "~/App_Data/Tmp/FileUploads";

        public virtual IHttpActionResult Get(string fileId)
        {
            var root = HttpContext.Current.Server.MapPath(path);
            var fullPath = string.Format("{0}/{1}", root, fileId);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(fullPath, FileMode.Open);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");
            return ResponseMessage(result);
        }    
    }
}