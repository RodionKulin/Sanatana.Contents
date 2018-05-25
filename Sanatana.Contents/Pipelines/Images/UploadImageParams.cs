using Sanatana.Contents.Files.Downloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Sanatana.Contents.Pipelines.Images
{
    public class UploadImageParams
    {
        public FileStream FileStream { get; set; }
        public string DownloadUrl { get; set; }
        public List<string> DestinationFileNames { get; set; }
        public List<ImageDestinationParams> Destinations { get; set; }
        public long? ContentLengthLimit { get; set; }
        public string UserId { get; set; }
    }
}
