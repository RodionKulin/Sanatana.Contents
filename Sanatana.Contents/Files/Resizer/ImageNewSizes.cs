using ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Files.Resizer
{
    public class ImageNewSizes
    {
        //properties
        public int Width { get; set; }
        public int Height { get; set; }
        public Rectangle CropRegion { get; set; }
        public Size Size { get; set; }
        public Size Padding { get; set; }


        //dependent properties
        public bool IsEmpty
        {
            get
            {
                return Size.IsEmpty;
            }
        }


        //init
        public ImageNewSizes(int width, int height)
        {
            Width = width;
            Height = height;
            Size = new Size(0, 0);
            Padding = new Size(0, 0);
            CropRegion = new Rectangle(0, 0, width, height);
        }
    }
}
