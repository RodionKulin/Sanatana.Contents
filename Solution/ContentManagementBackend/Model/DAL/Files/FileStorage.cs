using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Common.Utility;

namespace ContentManagementBackend
{
    public class FileStorage : IFileStorage
    {
        //свойства
        public string BaseUrl { get; set; }


        //методы
        public string GetBaseUrl()
        {
            return BaseUrl;
        }


        public Task<bool> Create(string namePath, byte[] inputBytes)
        {
            bool completed = false;

            try
            {
                FileInfo file = new FileInfo(namePath);
                if (file.Exists)
                {
                    file.Delete();
                }

                using (Stream fileStream = File.Create(namePath))
                {
                    fileStream.Write(inputBytes, 0, inputBytes.Length);
                }

                completed = true;
            }
            catch
            {
                completed = false;
            }

            return Task.FromResult(completed);
        }

        public Task<QueryResult<List<FileDetails>>> GetList(string path)
        {
            List<FileDetails> list = new List<FileDetails>();
            bool completed = false;

            try
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                FileInfo[] files = directory.GetFiles();

                foreach (FileInfo file in files)
                {
                    string key = file.FullName.Replace(BaseUrl, string.Empty);
                    list.Add(new FileDetails()
                    {
                        LastModifyTimeUtc = file.LastWriteTimeUtc,
                        Key = key
                    });
                }

                completed = true;
            }
            catch
            {
                completed = false;
            }

            if (list == null)
                list = new List<FileDetails>();

            var queryResult = new QueryResult<List<FileDetails>>(list, !completed);
            return Task.FromResult(queryResult);
        }

        public Task<QueryResult<bool>> Exists(string namePath)
        {
            bool exists = false;
            bool completed = false;

            try
            {
                exists = File.Exists(namePath);
                completed = true;
            }
            catch
            {
                completed = false;
            }

            var queryResult = new QueryResult<bool>(exists, !completed);
            return Task.FromResult(queryResult);
        }

        public Task<bool> Copy(List<string> oldNamePaths, List<string> newNamePaths)
        {
            bool completed = false;

            try
            {
                for (int i = 0; i < oldNamePaths.Count; i++)
                {
                    File.Copy(oldNamePaths[i], newNamePaths[i]);
                }

                completed = true;
            }
            catch
            {
                completed = false;
            }

            return Task.FromResult(completed);
        }

        public Task<bool> Move(string oldNamePath, string newNamePath)
        {
            bool completed = false;

            try
            {
                File.Move(oldNamePath, newNamePath);
                completed = true;
            }
            catch
            {
                completed = false;
            }

            return Task.FromResult(completed);
        }

        public Task<bool> Delete(List<string> namePaths)
        {
            bool completed = false;

            try
            {
                for (int i = 0; i < namePaths.Count; i++)
                {
                    FileInfo file = new FileInfo(namePaths[i]);
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }

                completed = true;
            }
            catch
            {
                completed = false;
            }

            return Task.FromResult(completed);
        }

        public Task<bool> Delete(string namePath)
        {
            return Delete(new List<string>() { namePath });
        }

        public Task<bool> DeleteDirectory(string folderPath)
        {
            bool completed = false;

            try
            {
                DirectoryInfo directory = new DirectoryInfo(folderPath);
                if(directory.Exists)
                {
                    directory.Delete();
                }

                completed = true;
            }
            catch
            {
                completed = false;
            }

            return Task.FromResult(completed);
        }

    }
}
