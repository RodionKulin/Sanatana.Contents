using Sanatana.Contents.Files.Downloads;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Sanatana.Contents.Pipelines.Images
{
    public class UploadImageParams
    {
        /// <summary>
        /// Input stream receiveing image file. One of DownloadUrl or FileStream is required.
        /// </summary>
        public Stream FileStream { get; set; }
        /// <summary>
        /// DownloadUrl to get image from. One of DownloadUrl or FileStream is required.
        /// </summary>
        public string DownloadUrl { get; set; }
        /// <summary>
        /// Destination format that image will be transformed to. If more than one provided multiple images will be created.
        /// </summary>
        public List<ImageDestinationParams> Destinations { get; set; }
        /// <summary>
        /// Optional input file length used to compare with FileLengthLimit.
        /// </summary>
        public long? FileLength { get; set; }
        /// <summary>
        /// Optional input file length limit.  If exceed will interrupt upload and return message. If input file length is not provided can insterrupt input Stream on exceeding the limit. 
        /// </summary>
        public long? FileLengthLimit { get; set; }
    }
}
