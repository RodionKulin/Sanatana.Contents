using Common.Utility.Pipelines;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ContentManagementBackend
{
    public class CommentImageQueries : FileQueriesBase
    {
        //инициализация
        public CommentImageQueries(IFileStorage fileStorage, IImageSettingsFactory settingsFactory)
            : base(fileStorage, null)
        {
            Settings = settingsFactory.GetSettings(Constants.IMAGE_SETTINGS_NAME_COMMENT);

            foreach (ImageTargetParameters item in Settings.Targets)
            {
                item.PathCreator.UrlBase = fileStorage.GetBaseUrl();
            }
        }


        //методы
        public virtual Task<PipelineResult<List<ImagePipelineResult>>> CreateStaticImage(
            HttpPostedFileBase file, string contentID)
        {
            string fileID = ShortGuid.NewGuid();
            return base.CreateStaticImage(file, contentID, fileID);
        }

        public virtual Task<PipelineResult<List<ImagePipelineResult>>> CreateStaticImage(
            string fileUrl, string contentID)
        {
            string fileID = ShortGuid.NewGuid();
            return base.CreateStaticImage(fileUrl, contentID, fileID);            
        }

    }
}
