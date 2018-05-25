using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Patterns.Pipelines;

namespace Sanatana.Contents.Files.Queries
{
    public class FileQueries : IFileQueries
    {
        //fields
        protected Dictionary<int, FilePathProvider> _filePathProviders;
        protected IFileStorage _fileStorage;
        


        //init
        public FileQueries(IEnumerable<FilePathProvider> filePathProviders, IFileStorage fileStorage)
        {
            _filePathProviders = filePathProviders.ToDictionary(x => x.FilePathProviderId);
            _fileStorage = fileStorage;
        }



        //Create
        public virtual Task Create(int pathProviderId, byte[] data, string directoryArg, string name)
        {
            FilePathProvider pathMapper = _filePathProviders[pathProviderId];
            string namePath = pathMapper.GetNamePath(directoryArg, name);
            return _fileStorage.Create(namePath, data);
        }



        //Select
        public virtual Task<bool> Exists(int pathProviderId, string directoryArg, string fileNameArg)
        {
            FilePathProvider pathMapper = _filePathProviders[pathProviderId];
            string namePath = pathMapper.GetNamePath(directoryArg, fileNameArg);
            return _fileStorage.Exists(namePath);
        }



        //Update
        public virtual Task Move(int oldMapperId, string oldDirectoryArg, string oldFileNameArg
            , int newMapperId, string newDirectoryArg, string newFileNameArg)
        {
            FilePathProvider oldPathMapper = _filePathProviders[oldMapperId];
            string oldPath = oldPathMapper.GetNamePath(oldDirectoryArg, oldFileNameArg);

            FilePathProvider newPathMapper = _filePathProviders[newMapperId];
            string newPath = newPathMapper.GetNamePath(newDirectoryArg, newFileNameArg);

            return _fileStorage.Move(new List<string>{ oldPath }, new List<string> { newPath });
        }

        

        //Delete
        public virtual Task DeleteFile(int pathProviderId, string directoryArg, string fileNameArg)
        {
            FilePathProvider pathMapper = _filePathProviders[pathProviderId];
            string namePath = pathMapper.GetNamePath(directoryArg, fileNameArg);
            return _fileStorage.Delete(new List<string>() { namePath });
        }

        public virtual Task DeleteDirectory(int pathProviderId, string directoryArg)
        {
            FilePathProvider pathMapper = _filePathProviders[pathProviderId];
            string directoryPath = pathMapper.GetDirectoryPath(directoryArg);
            return _fileStorage.DeleteDirectory(directoryPath);
        }
        
        public virtual async Task Clean(int pathProviderId, TimeSpan maxFileAge)
        {
            FilePathProvider pathMapper = _filePathProviders[pathProviderId];

            string rootDirectoryPath = pathMapper.GetRootDirectoryPath();
            List<FileDetails> files = await _fileStorage.GetList(rootDirectoryPath).ConfigureAwait(false);
           
            List<FileDetails> filesToDelete = new List<FileDetails>();
            foreach (FileDetails file in files)
            {
                TimeSpan fileAge = DateTime.UtcNow - file.LastModifyTimeUtc;
                if (fileAge > maxFileAge)
                {
                    filesToDelete.Add(file);
                }
            }

            if (filesToDelete.Count > 0)
            {
                List<string> keysToDelete = filesToDelete.Select(p => p.Key).ToList();
                await _fileStorage.Delete(keysToDelete).ConfigureAwait(false);
            }
        }
    }
}
