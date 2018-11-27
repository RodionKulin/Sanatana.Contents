using SixLabors.Primitives;
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
        /// <summary>
        /// Target image final width.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Target image final height.
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Target size that will be used to cut actual image sizes.
        /// </summary>
        public Rectangle CropRegion { get; set; }
        /// <summary>
        /// Target size that will be used to squeeze or stretch image.
        /// </summary>
        public Size Size { get; set; }
        /// <summary>
        /// Target size that will be filled with white space if actual image size is not enough.
        /// </summary>
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
            Size = new Size(width, height);
            Padding = new Size(0, 0);
            CropRegion = new Rectangle(0, 0, width, height);
        }
    }
}
