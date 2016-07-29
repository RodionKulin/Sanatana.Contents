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
    public class PreviewImageQueries : FileQueriesBase
    {
        //инициализация
        public PreviewImageQueries(IFileStorage fileStorage, IImageSettingsFactory settingsFactory)
            : base(fileStorage, null)
        {
            Settings = settingsFactory.GetSettings(Constants.IMAGE_SETTINGS_NAME_PREVIEW);
            
            foreach (ImageTargetParameters item in Settings.Targets)
            {
                item.PathCreator.UrlBase = fileStorage.GetBaseUrl();
            }
        }

        
        //методы        

        public string GetImageUrl<TKey>(ContentBase<TKey> content, ContentSubmitVM submitVM, bool useRandomSuffix)
            where TKey : struct
        {
            if(submitVM.ImageStatus == ImageStatus.NotSet
                || (submitVM.ImageStatus == ImageStatus.Temp && submitVM.ImageID == null))
            {
                return null;
            }
            
            string url = submitVM.ImageStatus == ImageStatus.Static
                ? PathCreator.CreateStaticUrl(content.Url)
                : PathCreator.CreateTempUrl(content.AuthorID.ToString(), submitVM.ImageID.Value);

            if(useRandomSuffix)
            {
                url += "?r=" + ShortGuid.NewGuid();
            }

            return url;
        }

        public virtual Task<PipelineResult<List<ImagePipelineResult>>> CreateStaticImage(
            string fileUrl, string contentUrl)
        {
            return base.CreateStaticImage(fileUrl, contentUrl);
        }

        public virtual Task<PipelineResult<List<ImagePipelineResult>>> CreateStaticImage(
            HttpPostedFileBase file, string contentUrl)
        {
            return base.CreateStaticImage(file, fileUrl: null, namePathParts: contentUrl);
        }
        
    }
}
