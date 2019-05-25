using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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


        //files
        public virtual Task Create(string namePath, byte[] inputBytes)
        {
            namePath = namePath.Replace('/', '\\');
            string fullPath = Path.Combine(_settings.BaseDirectory, namePath);
            FileInfo file = new FileInfo(fullPath);
            if (file.Exists)
            {
                file.Delete();
            }

            if (file.Directory.Exists == false)
            {
                file.Directory.Create();
            }

            using (Stream fileStream = File.Create(fullPath))
            {
                fileStream.Write(inputBytes, 0, inputBytes.Length);
            }

            return Task.FromResult(0);
        }

        public virtual Task<bool> Exists(string namePath)
        {
            namePath = namePath.Replace('/', '\\');
            string fullPath = Path.Combine(_settings.BaseDirectory, namePath);
            bool exists = File.Exists(fullPath);

            return Task.FromResult(exists);
        }

        public virtual Task Copy(List<string> oldNamePaths, List<string> newNamePaths)
        {
            for (int i = 0; i < oldNamePaths.Count; i++)
            {
                string oldFullPath = Path.Combine(_settings.BaseDirectory, oldNamePaths[i].Replace('/', '\\'));
                string newFullPath = Path.Combine(_settings.BaseDirectory, newNamePaths[i].Replace('/', '\\'));

                DirectoryInfo directory = new DirectoryInfo(newFullPath);
                if (directory.Exists == false)
                {
                    directory.Create();
                }

                File.Copy(oldFullPath, newFullPath);
            }

            return Task.FromResult(0);
        }

        public virtual Task Move(List<string> oldNamePaths, List<string> newNamePaths)
        {
            for (int i = 0; i < oldNamePaths.Count; i++)
            {
                string oldFullPath = Path.Combine(_settings.BaseDirectory, oldNamePaths[i].Replace('/', '\\'));
                string newFullPath = Path.Combine(_settings.BaseDirectory, newNamePaths[i].Replace('/', '\\'));

                DirectoryInfo directory = new DirectoryInfo(newFullPath);
                if (directory.Exists == false)
                {
                    directory.Create();
                }

                File.Move(oldFullPath, newFullPath);
            }

            return Task.FromResult(0);
        }

        public virtual Task Delete(List<string> namePaths)
        {
            for (int i = 0; i < namePaths.Count; i++)
            {
                string fullPath = Path.Combine(_settings.BaseDirectory, namePaths[i].Replace('/', '\\'));
                FileInfo file = new FileInfo(fullPath);
                if (file.Exists)
                {
                    file.Delete();
                }

            }

            return Task.FromResult(0);
        }



        //directories
        public virtual Task<List<FileDetails>> GetList(string directoryPath)
        {
            List<FileDetails> list = new List<FileDetails>();

            directoryPath = directoryPath.Replace('/', '\\');
            string fullPath = Path.Combine(_settings.BaseDirectory, directoryPath);
            DirectoryInfo directory = new DirectoryInfo(fullPath);
            if (directory.Exists == false)
            {
                return Task.FromResult(list);
            }

            FileInfo[] files = directory.GetFiles();
            foreach (FileInfo file in files)
            {
                string key = file.FullName.Replace(_settings.BaseDirectory, string.Empty);
                list.Add(new FileDetails()
                {
                    LastModifyTimeUtc = file.LastWriteTimeUtc,
                    NamePath = key
                });
            }

            return Task.FromResult(list);
        }

        public virtual Task DeleteDirectory(string directoryPath)
        {
            directoryPath = directoryPath.Replace('/', '\\');
            string fullPath = Path.Combine(_settings.BaseDirectory, directoryPath);
            DirectoryInfo directory = new DirectoryInfo(fullPath);
            if (directory.Exists)
            {
                directory.Delete();
            }

            return Task.FromResult(0);
        }

    }
}
