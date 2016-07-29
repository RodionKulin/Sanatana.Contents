using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class ImageSettingsFactory : IImageSettingsFactory
    {
        //поля
        private IEnumerable<ImageSettings> _imageSettings;



        //инициализация
        public ImageSettingsFactory(IEnumerable<ImageSettings> imageSettings)
        {
            _imageSettings = imageSettings;
        }



        //методы
        public ImageSettings GetSettings(string name)
        {
            return _imageSettings.FirstOrDefault(p => p.Name == name);
        }
    }
}
