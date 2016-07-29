using Common.Utility.Pipelines;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ContentManagementBackend
{
    public class ContentImageQueries : FileQueriesBase
    {
        //поля
        protected ImageResizing _imageUtility;

        
        
        //инициализация
        public ContentImageQueries(IFileStorage fileStorage, IImageSettingsFactory settingsFactory)
            : base(fileStorage, null)
        {
            _imageUtility = new ImageResizing();                       
            Settings = settingsFactory.GetSettings(Constants.IMAGE_SETTINGS_NAME_CONTENT);

            foreach (ImageTargetParameters item in Settings.Targets)
            {
                item.PathCreator.UrlBase = fileStorage.GetBaseUrl();
            }
        }


        
        //методы
        public virtual async Task<bool> MoveTempToStatic(List<HtmlElement> content, string contentID)
        {
            List<HtmlImageInfo> images = new List<HtmlImageInfo>();
            images.AddRange(content.SelectMany(p => HtmlMediaExtractor.FindImages(p)));                
            List<string> allImageUrls = images.Select(p => p.Src).ToList();


            //move files
            bool completed = await MoveTempToStatic(allImageUrls, contentID);
            if (!completed)
            {
                return false;
            }


            //update html
            List<string> newImageUrls = ReplceTempToStaticUrls(allImageUrls, contentID);
            for (int i = 0; i < images.Count; i++)
            {
                images[i].Src = newImageUrls[i];
            }

            return true;
        }

        protected virtual async Task<bool> MoveTempToStatic(List<string> allImageUrls, string contentID)
        {
            string tempBaseUrl = PathCreator.CreateTempUrlRoot();
            string staticBaseUrl = PathCreator.CreateStaticUrlRoot();
            string urlBase = FileStorage.GetBaseUrl();

            List<string> tmpUrls = allImageUrls.Where(p => p.StartsWith(tempBaseUrl)).ToList();
            List<string> staticUrls = allImageUrls.Where(p => p.StartsWith(staticBaseUrl)).ToList();


            //get existing static
            string staticFolderPath = PathCreator.CreateStaticFolderPath(contentID);
            QueryResult<List<FileDetails>> storageStaticImages = await FileStorage.GetList(staticFolderPath);
            if (storageStaticImages.HasExceptions)
            {
                return false;
            }

            List<string> removeStaticKeys = storageStaticImages.Result
                .Where(stored => !staticUrls.Any(posted => posted.EndsWith(stored.Key)))
                .Select(p => p.Key)
                .ToList();


            //copy temp
            List<string> tempNamePaths = tmpUrls
                .Select(p => p.Replace(urlBase, string.Empty)).ToList();

            bool copyCompleted = await CopyTempToStatic(tempNamePaths, contentID);
            if (!copyCompleted)
            {
                return false;
            }


            //delete
            List<string> namePathsToRemove = new List<string>(removeStaticKeys);
            namePathsToRemove.AddRange(tempNamePaths);

            if (namePathsToRemove.Count > 0)
            {
                bool deleteCompleted = await FileStorage.Delete(namePathsToRemove);
                if (!deleteCompleted)
                {
                    return false;
                }
            }

            return true;
        }

        protected virtual Task<bool> CopyTempToStatic(List<string> tempNamePaths, string contentID)
        {
            List<string> oldNamePaths = new List<string>();
            List<string> newNamePaths = new List<string>();

            foreach (string tempNamePath in tempNamePaths)
            {
                Guid? fileID = PathCreator.ExtractFileIDFromTemp(tempNamePath);
                if (fileID == null)
                    continue;

                string shortFileID = new ShortGuid(fileID.Value);
                string staticNamePath = PathCreator.CreateStaticNamePath(contentID, shortFileID);

                oldNamePaths.Add(tempNamePath);
                newNamePaths.Add(staticNamePath);
            }

            return oldNamePaths.Count > 0
                ? FileStorage.Copy(oldNamePaths, newNamePaths)
                : Task.FromResult(true);
        }

        protected virtual List<string> ReplceTempToStaticUrls(List<string> urls, string contentID)
        {
            List<string> result = new List<string>();
            string tempBaseUrl = PathCreator.CreateTempUrlRoot();

            foreach (string url in urls)
            {
                if (!url.StartsWith(tempBaseUrl))
                {
                    result.Add(url);
                    continue;
                }

                Guid? fileID = PathCreator.ExtractFileIDFromTemp(url);
                if (fileID == null)
                {
                    result.Add(string.Empty);
                    continue;
                }

                string shortFileID = new ShortGuid(fileID.Value);
                string staticUrl = PathCreator.CreateStaticUrl(contentID, shortFileID);
                result.Add(staticUrl);
            }

            return result;
        }      

    }
}
