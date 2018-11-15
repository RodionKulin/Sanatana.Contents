using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Sanatana.Patterns.Pipelines;
using System.IO;
using Sanatana.Contents.Html.Media;
using Sanatana.Contents.Html.HtmlNodes;
using Sanatana.Contents.Utilities;
using Sanatana.Contents.Files.Queries;
using Sanatana.Contents.Extensions;

namespace Sanatana.Contents.Files.Services
{
    public class ImageFileService : IImageFileService
    {
        //fields
        protected Dictionary<int, FilePathProvider> _filePathProviders;
        protected IFileStorage _fileStorage;
        protected IHtmlMediaExtractor _htmlMediaExtractor;
        

        //init
        public ImageFileService(IEnumerable<FilePathProvider> filePathProviders, IFileStorage fileStorage
            , IHtmlMediaExtractor htmlMediaExtractor)
        {
            _htmlMediaExtractor = htmlMediaExtractor;
            _filePathProviders = filePathProviders.ToDictionaryOrThrow();
            _fileStorage = fileStorage;
        }

        
        
        //methods
        public virtual async Task UpdateContentImages(int pathProviderId, List<HtmlElement> content, string newDirectoryArg)
        {
            FilePathProvider pathMapper = _filePathProviders[pathProviderId];

            List<HtmlImageInfo> images = content.SelectMany(p => _htmlMediaExtractor.FindImages(p)).ToList();
            List<string> currentUrls = images.Select(p => p.Src).ToList();
            
            IEnumerable<string> destinationCurrentUrls = SelectMatchingRoot(pathProviderId, currentUrls);
            IEnumerable<string> destinationCurrentFilePaths = destinationCurrentUrls
                .Select(x => pathMapper.TrimToRelativeUrl(x))
                .ToList();
            IEnumerable<string> filesToMoveUrls = currentUrls.Except(destinationCurrentUrls);
            List<string> filesToMoveFilePaths = filesToMoveUrls
                .Select(x => pathMapper.TrimToRelativeUrl(x))
                .ToList();

            //remove unused files
            List<FileDetails> unusedFiles = await GetUnusedStoredKeys(
                pathProviderId, destinationCurrentFilePaths, newDirectoryArg);
            List<string> fileToRemoveFilePaths = unusedFiles.Select(x => x.Key).ToList();
            await _fileStorage.Delete(fileToRemoveFilePaths);

            //move temp file to destination path
            (List<string> filePaths, List<string> urls) newPaths =
                GenerateRelativePathsAndUrls(pathProviderId, newDirectoryArg, filesToMoveFilePaths.Count);
            await _fileStorage.Move(filesToMoveFilePaths, newPaths.filePaths);
            for (int i = 0; i < images.Count; i++)
            {
                bool isUrlUpdated = filesToMoveUrls.Contains(currentUrls[i]);
                if (isUrlUpdated)
                {
                    images[i].Src = newPaths.urls[i];
                }
            }
        }
        
        public virtual (List<string>, List<string>) GenerateRelativePathsAndUrls(int pathProviderId, string directoryArg, int count)
        {
            FilePathProvider pathMapper = _filePathProviders[pathProviderId];

            List<string> newRelativeNamePaths = new List<string>(count);
            List<string> newUrls = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                string[] directoryAndNameFormatArgs = new[] {
                    directoryArg,               //set directory argument
                    ShortGuid.NewGuid().Value   //generate new ShortGuid name argument
                };

                string newFilePaths = pathMapper.GetPathAndName(directoryAndNameFormatArgs);
                newRelativeNamePaths.Add(newFilePaths);

                string newUrl = pathMapper.GetFullUrl(directoryAndNameFormatArgs);
                newUrls.Add(newUrl);
            }

            return (newRelativeNamePaths, newUrls);
        }

        protected virtual IEnumerable<string> SelectMatchingRoot(int pathProviderId, List<string> imageUrls)
        {
            FilePathProvider pathMapper = _filePathProviders[pathProviderId];
            string rootDirectoryUrl = pathMapper.GetRootDirectoryUrl();

            IEnumerable<string> matchedUrls = imageUrls
                .Where(p => p.StartsWith(rootDirectoryUrl));

            return matchedUrls;
        }

        public virtual async Task<List<FileDetails>> GetUnusedStoredKeys(
            int pathProviderId, IEnumerable<string> currentFilePaths, string directoryArg)
        {
            FilePathProvider pathMapper = _filePathProviders[pathProviderId];

            string directoryPath = pathMapper.GetPath(directoryArg);
            List<FileDetails> storageStaticImages = await _fileStorage.GetList(directoryPath)
                .ConfigureAwait(false);

            List<FileDetails> unusedStoredKeys = storageStaticImages
                .Where(storedFile => currentFilePaths
                    .Any(currentFilePath => currentFilePath == storedFile.Key) == false)
                .ToList();

            return unusedStoredKeys;
        }


    }
}
