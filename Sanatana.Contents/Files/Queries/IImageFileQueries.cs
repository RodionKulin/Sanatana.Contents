using Sanatana.Contents.Html.HtmlNodes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sanatana.Contents.Files.Queries
{
    public interface IImageFileQueries
    {
        (List<string>, List<string>) GenerateRelativePathsAndUrls(int pathProviderId, string directoryArg, int count);
        Task<List<FileDetails>> GetUnusedStoredKeys(int pathProviderId, IEnumerable<string> currentFilePaths, string directoryArg);
        Task UpdateContentImages(int pathProviderId, List<HtmlElement> content, string newDirectoryArg);
    }
}