using Sanatana.Contents.Files.Resizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Pipelines.Images
{
    [Serializable]
    public class ImageDestinationParams
    {
        //properties
        /// <summary>
        /// Target width that will be used with ResizeType to adjust image size.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Target height that will be used with ResizeType to adjust image size.
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Round image corners.
        /// </summary>
        public bool RoundCorners { get; set; }
        /// <summary>
        /// Format used to store image as a file.
        /// </summary>
        public ImageFormat TargetFormat { get; set; }
        /// <summary>
        /// Resizing type to adjust image width and height before storing it.
        /// </summary>
        public ImageResizeType ResizeType { get; set; }
        /// <summary>
        /// Id for FilePathProvider that should be registered in dependency container. FilePathProvider stores parameters to construct full url for the image.
        /// </summary>
        public int FilePathProviderId { get; set; }
        /// <summary>
        /// Optional file name without extension. If not provided new ShortGuid will be used.
        /// </summary>
        public string DestinationFileName { get; set; }
        /// <summary>
        /// Optional. If provided is used as a directory name to put file into. Number of parameters for string.Format function depends on format supplied to FilePathProvider selected.
        /// </summary>
        public string[] RelativePathArgs { get; set; }


        //methods
        public virtual ImageDestinationParams Clone()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, this);
                stream.Seek(0, SeekOrigin.Begin);
                return (ImageDestinationParams)formatter.Deserialize(stream);
            }
        }
        public virtual ImageDestinationParams Clone(
            string[] relativePathArgs = null, string destinationFileName = null)
        {
            ImageDestinationParams clone = Clone();
            clone.RelativePathArgs = relativePathArgs;
            clone.DestinationFileName = destinationFileName;
            return clone;
        }
    }
}
