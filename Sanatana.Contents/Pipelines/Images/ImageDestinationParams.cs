using Sanatana.Contents.Files.Resizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Pipelines.Images
{
    public class ImageDestinationParams
    {
        //properties
        public int Width { get; set; }
        public int Height { get; set; }
        public bool RoundCorners { get; set; }
        public ImageFormat TargetFormat { get; set; }
        public ImageResizeType ResizeType { get; set; }
        public int FilePathMapperId { get; set; }

        
    }
}
