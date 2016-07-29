using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class AvatarImageSettings : ImageSettings
    {
        //свойства
        public List<string> DefaultImages { get; set; }
        public new static AvatarImageSettings Default
        {
            get
            {
                return new AvatarImageSettings()
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
                            Height = 100,
                            Width = 100
                        }
                    }
                };
            }
        }



        //инициализация
        public static AvatarImageSettings CreateDefault(List<string> defaultImages)
        {
            AvatarImageSettings settings = AvatarImageSettings.Default;
            settings.DefaultImages = defaultImages;
            return settings;
        }

    }
}
