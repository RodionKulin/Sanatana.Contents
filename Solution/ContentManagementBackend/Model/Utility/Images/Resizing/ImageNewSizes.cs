using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    internal class ImageNewSizes
    {
        //свойства
        public int Width { get; set; }
        public int Height { get; set; }
        public Rectangle CropRegion { get; set; }
        public Rectangle DrawingRegion { get; set; }


        //зависимые свойства
        public bool IsEmpty
        {
            get
            {
                return DrawingRegion.IsEmpty;
            }
        }


        //инициализация
        public ImageNewSizes(int width, int height)
        {
            Width = width;
            Height = height;
            DrawingRegion = new Rectangle(0, 0, width, height);
            CropRegion = new Rectangle(0, 0, width, height);
        }
    }
}
