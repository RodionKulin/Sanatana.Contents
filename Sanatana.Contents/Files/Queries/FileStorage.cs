using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Contents.Resources;

namespace Sanatana.Contents.Files.Queries
{
    public class FileStorage : IFileStorage
    {
        //fields
        protected FileStorageSettings _settings;


        //init
        public FileStorage(FileStorageSettings settings)
        {
            _settings = settings;
        }


        //methods
        public virtual Task Create(string namePath, byte[] inputBytes)
        {
            string fullPath = Path.Combine(_settings.Directory, namePath);
            FileInfo file = new FileInfo(fullPath);
            if (file.Exists)
            {
                file.Delete();
            }

            using (Stream fileStream = File.Create(fullPath))
            {
                fileStream.Write(inputBytes, 0, inputBytes.Length);
            }

            return Task.FromResult(0);
        }

        public virtual Task<List<FileDetails>> GetList(string directoryPath)
        {
            List<FileDetails> list = new List<FileDetails>();

            string fullPath = Path.Combine(_settings.Directory, directoryPath);
            DirectoryInfo directory = new DirectoryInfo(fullPath);
            FileInfo[] files = directory.GetFiles();

            foreach (FileInfo file in files)
            {
                string key = file.FullName.Replace(_settings.Directory, string.Empty);
                list.Add(new FileDetails()
                {
                    LastModifyTimeUtc = file.LastWriteTimeUtc,
                    Key = key
                });
            }

            return Task.FromResult(list);
        }

        public virtual Task<bool> Exists(string namePath)
        {
            string fullPath = Path.Combine(_settings.Directory, namePath);
            bool exists = File.Exists(fullPath);

            return Task.FromResult(exists);
        }

        public virtual Task Copy(List<string> oldNamePaths, List<string> newNamePaths)
        {
            for (int i = 0; i < oldNamePaths.Count; i++)
            {
                string oldFullPath = Path.Combine(_settings.Directory, oldNamePaths[i]);
                string newFullPath = Path.Combine(_settings.Directory, newNamePaths[i]);
                File.Copy(oldFullPath, newFullPath);
            }

            return Task.FromResult(0);
        }

        public virtual Task Move(List<string> oldNamePaths, List<string> newNamePaths)
        {
            for (int i = 0; i < oldNamePaths.Count; i++)
            {
                string oldFullPath = Path.Combine(_settings.Directory, oldNamePaths[i]);
                string newFullPath = Path.Combine(_settings.Directory, newNamePaths[i]);
                File.Move(oldFullPath, newFullPath);
            }

            return Task.FromResult(0);
        }

        public virtual Task Delete(List<string> namePaths)
        {
            for (int i = 0; i < namePaths.Count; i++)
            {
                string fullPath = Path.Combine(_settings.Directory, namePaths[i]);
                FileInfo file = new FileInfo(fullPath);
                if (file.Exists)
                {
                    file.Delete();
                }
            }

            return Task.FromResult(0);
        }

        public virtual Task Delete(string namePath)
        {
            return Delete(new List<string>() { namePath });
        }

        public virtual Task DeleteDirectory(string directoryPath)
        {
            string fullPath = Path.Combine(_settings.Directory, directoryPath);
            DirectoryInfo directory = new DirectoryInfo(fullPath);
            if (directory.Exists)
            {
                directory.Delete();
            }

            return Task.FromResult(0);
        }

    }
}
