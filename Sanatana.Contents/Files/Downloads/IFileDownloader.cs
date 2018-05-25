using Sanatana.Patterns.Pipelines;
using System.IO;
using System.Threading.Tasks;

namespace Sanatana.Contents.Files.Downloads
{
    public interface IFileDownloader
    {
        Task<PipelineResult<byte[]>> Download(string url, long? sizeLimit);
        Task<PipelineResult<byte[]>> Download(Stream inputStream, long contentLength, long? sizeLimit);
    }
}