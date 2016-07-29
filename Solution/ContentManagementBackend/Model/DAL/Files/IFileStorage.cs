using Common.Utility;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public interface IFileStorage
    {
        string GetBaseUrl();
        Task<bool> Create(string namePath, byte[] inputBytes);
        Task<QueryResult<bool>> Exists(string namePath);
        Task<QueryResult<List<FileDetails>>> GetList(string path);
        Task<bool> Move(string oldNamePath, string newNamePath);
        Task<bool> Copy(List<string> oldNamePaths, List<string> newNamePaths);
        Task<bool> Delete(List<string> namePaths);
        Task<bool> DeleteDirectory(string folderPath);
    }
}