using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class ImageTargetParameters
    {
        //свойства
        public int Width { get; set; }
        public int Height { get; set; }
        public bool RoundCorners { get; set; }
        public ImageFormat TargetFormat { get; set; }
        public ImageResizeType ResizeType { get; set; }
        public PathCreator PathCreator { get; set; }

        
    }
}
