using System.Threading.Tasks;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Patterns.Pipelines;

namespace Sanatana.Contents.Pipelines.Contents
{
    public interface IInsertContentPipeline<TKey, TCategory, TContent>
        : IContentPipelineBase<TKey, TCategory, TContent>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
    {
        Task<bool> ValidateInput(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context);
    }
}