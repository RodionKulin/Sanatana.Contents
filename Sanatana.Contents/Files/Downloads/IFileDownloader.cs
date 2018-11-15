using Sanatana.Patterns.Pipelines;
using System.IO;
using System.Threading.Tasks;

namespace Sanatana.Contents.Files.Downloads
{
    public interface IFileDownloader
    {
        Task<PipelineResult<byte[]>> Download(string url, long? lengthLimit);
        Task<PipelineResult<byte[]>> Download(Stream inputStream, long? contentLength, long? lengthLimit);
    }
}