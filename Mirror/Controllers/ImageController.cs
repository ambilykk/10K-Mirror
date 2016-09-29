using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Mirror.Controllers
{
    public class ImageController : ApiController
    {
        public async Task<string> GetSentiment(string message)
        {
            return await ImageHelper.GetSentiments(message);
        }

        public async Task<string> PostFormData()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string path = Path.GetTempPath();
            //string path = HttpContext.Current.Server.MapPath("~/App_Data");    
            var provider = new MultipartFormDataStreamProvider(path);

            try
            {
                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);

                // This illustrates how to get the file names.
                foreach (MultipartFileData file in provider.FileData)
                {                    
                    var result = await ImageHelper.AnalyzeEmotion(file.LocalFileName);                    
                    File.Delete(file.LocalFileName);
                    return result;
                }
                return "ok";
            }
            catch (System.Exception e)
            {
                return e.Message;
            }
        }
    }
}
