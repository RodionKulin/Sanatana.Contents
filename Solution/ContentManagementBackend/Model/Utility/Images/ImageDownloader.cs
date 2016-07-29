using ContentManagementBackend.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ContentManagementBackend
{
    public class ImageDownloader
    {
        //методы
        public static byte[] ReceiveStream(Stream inputStream, long contentLength, long sizeLimit, out string error)
        {
            if(sizeLimit > 0 && contentLength > sizeLimit)
            {
                error = string.Format(MessageResources.Image_SizeExceeeded, sizeLimit);
                return null;
            }
            
            return ReadStream(inputStream, sizeLimit, out error);
        }
        
        public static byte[] Download(string url, long sizeLimit, out string error)
        {
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "http://" + url;
            }
            
            Uri uri;
            if(!Uri.TryCreate(url, UriKind.Absolute, out uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                error = string.Format(MessageResources.Image_InvalidUrl, url);
                return null;
            }

            if(sizeLimit > 0)
            {
                long? contentLength = DetermineContentLength(uri);
                if (contentLength != null && contentLength > sizeLimit)
                {
                    error = string.Format(MessageResources.Image_SizeExceeeded, sizeLimit);
                    return null;
                }
            }

            using (WebClient wc = new WebClient())
            {
               Stream fileStream;
                try
                {
                    fileStream = wc.OpenRead(uri);
                    return ReadStream(fileStream, sizeLimit, out error);
                }
                catch
                {
                    error = string.Format(MessageResources.Image_ReceiveException, sizeLimit);
                    return null;
                }
            }
        }

        private static byte[] ReadStream(Stream input, long sizeLimit, out string error)
        {
            List<byte> streamBytes = new List<byte>();
            
            error = null;

            try
            {
                byte[] buffer = new byte[16 * 1024];
                int read;

                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    streamBytes.AddRange(buffer.Take(read));
                    
                    if (sizeLimit > 0 && streamBytes.Count > sizeLimit)
                    {
                        error = string.Format(MessageResources.Image_SizeExceeeded, sizeLimit);
                        break;
                    }
                }
            }
            catch
            {
                error = MessageResources.Image_ReceiveException;
            }
            
            return streamBytes.ToArray();
        }

        private static long? DetermineContentLength(Uri url)
        {
            try
            {
                WebRequest req = HttpWebRequest.Create(url);
                req.Method = "HEAD";

                using (WebResponse resp = req.GetResponse())
                {
                    long size;
                    if (long.TryParse(resp.Headers.Get("Content-Length"), out size))
                    {
                        return size;
                    }
                }
            }
            catch
            {

            }
            return null;
        }
    }
}
