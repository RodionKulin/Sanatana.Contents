using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class ImageSettings
    { 
        //свойства
        public string Name { get; set; }
        public long SizeLimit { get; set; }
        public TimeSpan TempDeleteAge { get; set; }
        public List<ImageTargetParameters> Targets { get; set; }



        //инициализация
        public static ImageSettings Default
        {
            get
            {
                return new ImageSettings()
                {
                    TempDeleteAge = TimeSpan.FromDays(1),
                    SizeLimit = 5124000,
                    Targets = new List<ImageTargetParameters>()
                    {
                        new ImageTargetParameters()
                        {
                            TargetFormat = ImageFormat.Jpeg,
                            ResizeType = ImageResizeType.FitAndFill,
                            RoundCorners = true,
                            Height = 640,
                            Width = 360
                        }
                    }

                };
            }
        }
    }
}
