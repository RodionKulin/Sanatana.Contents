using Common.Utility.Pipelines;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ContentManagementBackend
{
    public class AvatarImageQueries : FileQueriesBase
    {
        //поля
        protected static Random _random;



        //инициализация
        static AvatarImageQueries()
        {
            _random = new Random(DateTime.UtcNow.Millisecond);
        }
        public AvatarImageQueries(IFileStorage fileStorage, AvatarImageSettings settings)
            :base(fileStorage, settings)
        {
            Settings = settings;

            foreach (ImageTargetParameters item in settings.Targets)
            {
                item.PathCreator.UrlBase = fileStorage.GetBaseUrl();
            }
        }



        //методы
        public virtual string GetDefaultAvatar()
        {
            var settings = (AvatarImageSettings)Settings;

            if (settings.DefaultImages == null || settings.DefaultImages.Count == 0)
            {
                return null;
            }

            int index = _random.Next(settings.DefaultImages.Count);
            return settings.DefaultImages[index];
        }

        public virtual Task<PipelineResult<List<ImagePipelineResult>>> CreateStaticImage(
            string fileUrl, string avatarName)
        {
            return base.CreateStaticImage(fileUrl, avatarName);
        }
    }
}
