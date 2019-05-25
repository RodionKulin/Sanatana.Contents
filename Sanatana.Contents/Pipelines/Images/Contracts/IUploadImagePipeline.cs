using System.Collections.Generic;
using System.Threading.Tasks;
using Sanatana.Patterns.Pipelines;

namespace Sanatana.Contents.Pipelines.Images
{
    public interface IUploadImagePipeline
        : IPipeline<UploadImageParams, PipelineResult<List<UploadImageResult>>>
    {
        Task<bool> CheckExistingNames(PipelineContext<UploadImageParams, PipelineResult<List<UploadImageResult>>> context);
        Task<bool> CreateFile(PipelineContext<UploadImageParams, PipelineResult<List<UploadImageResult>>> context);
        Task<bool> Download(PipelineContext<UploadImageParams, PipelineResult<List<UploadImageResult>>> context);
        Task<bool> Format(PipelineContext<UploadImageParams, PipelineResult<List<UploadImageResult>>> context);
        Task<bool> GenerateTargetsNamePaths(PipelineContext<UploadImageParams, PipelineResult<List<UploadImageResult>>> context);
    }
}