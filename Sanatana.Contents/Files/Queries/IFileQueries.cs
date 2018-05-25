using System;
using System.Threading.Tasks;

namespace Sanatana.Contents.Files.Queries
{
    public interface IFileQueries
    {
        Task Clean(int pathProviderId, TimeSpan maxFileAge);
        Task Create(int pathProviderId, byte[] data, string directoryArg, string name);
        Task DeleteDirectory(int pathProviderId, string directoryArg);
        Task DeleteFile(int pathProviderId, string directoryArg, string fileNameArg);
        Task<bool> Exists(int pathProviderId, string directoryArg, string fileNameArg);
        Task Move(int oldMapperId, string oldDirectoryArg, string oldFileNameArg, int newMapperId, string newDirectoryArg, string newFileNameArg);
    }
}