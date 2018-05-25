using Sanatana.Contents.Resources;
using Sanatana.Patterns.Pipelines;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace Sanatana.Contents.Files.Downloads
{
    public class FileDownloader : IFileDownloader
    {
        //methods
        public virtual Task<PipelineResult<byte[]>> Download(Stream inputStream, long contentLength, long? contentLengthLimit)
        {
            PipelineResult<byte[]> result = null;

            if (contentLengthLimit != null && contentLength > contentLengthLimit)
            {
                string error = string.Format(ContentsMessages.Image_SizeExceeeded, contentLengthLimit);
                result = PipelineResult<byte[]>.Error(error);
            }
            else
            {
                result = ReadStream(inputStream, contentLengthLimit);
            }
            
            return Task.FromResult(result);
        }
        
        public virtual async Task<PipelineResult<byte[]>> Download(string url, long? sizeLimit)
        {
            string lowcaseUrl = url.ToLowerInvariant();
            if (!lowcaseUrl.StartsWith("http://") && !lowcaseUrl.StartsWith("https://"))
            {
                url = "http://" + url;
            }
            
            Uri uri;
            if(!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                string error = string.Format(ContentsMessages.Image_InvalidUrl, url);
                return PipelineResult<byte[]>.Error(error);
            }

            if(sizeLimit != null)
            {
                long? contentLength = await DetermineContentLength(uri).ConfigureAwait(false);
                if (contentLength != null && contentLength > sizeLimit)
                {
                    string error = string.Format(ContentsMessages.Image_SizeExceeeded, sizeLimit);
                    return PipelineResult<byte[]>.Error(error);
                }
            }

            using (HttpClient client = new HttpClient())
            {
                Stream fileStream = await client.GetStreamAsync(uri).ConfigureAwait(false);
                return ReadStream(fileStream, sizeLimit);
            }
        }

        protected virtual PipelineResult<byte[]> ReadStream(Stream input, long? sizeLimit)
        {
            List<byte> streamBytes = new List<byte>();
           
            byte[] buffer = new byte[16 * 1024];
            int read;

            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                streamBytes.AddRange(buffer.Take(read));
                    
                if (sizeLimit != null && streamBytes.Count > sizeLimit)
                {
                    string error = string.Format(ContentsMessages.Image_SizeExceeeded, sizeLimit);
                    return PipelineResult<byte[]>.Error(error);
                }
            }
           
            byte[] streamByteArray = streamBytes.ToArray();
            return PipelineResult<byte[]>.Success(streamByteArray);
        }

        protected virtual async Task<long?> DetermineContentLength(Uri url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.SendAsync(
                        new HttpRequestMessage(HttpMethod.Head, url)
                        , HttpCompletionOption.ResponseHeadersRead)
                        .ConfigureAwait(false);

                   KeyValuePair<string, IEnumerable<string>> contentLengthHeader = response.Headers.FirstOrDefault(
                        h => string.Compare(h.Key, "Content-Length", true) == 0);

                    return long.Parse(contentLengthHeader.Value.First());
                }
            }
            catch
            {

            }
            return null;
        }
    }
}
