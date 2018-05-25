using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sanatana.Contents.Files.Queries
{
    public interface IFileStorage
    {
        Task Create(string namePath, byte[] inputBytes);
        Task<bool> Exists(string namePath);
        Task<List<FileDetails>> GetList(string path);
        Task Move(List<string> oldNamePaths, List<string> newNamePaths);
        Task Copy(List<string> oldNamePaths, List<string> newNamePaths);
        Task Delete(List<string> namePaths);
        Task DeleteDirectory(string directoryPath);
    }
}