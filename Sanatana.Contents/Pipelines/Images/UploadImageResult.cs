using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Pipelines.Images
{
    public class UploadImageResult
    {
        /// <summary>
        /// Full url constructed by FilePathProvider
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// File name without extension provided in ImageDestinationParams.DestinationFileName or ShortGuid if not provided.
        /// </summary>
        public string FileName { get; set; }
    }
}
