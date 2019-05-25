using System.Threading.Tasks;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Patterns.Pipelines;

namespace Sanatana.Contents.Pipelines.Contents
{
    public interface IUpdateContentPipeline<TKey, TCategory, TContent>
        : IContentPipelineBase<TKey, TCategory, TContent>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
    {
        Task<bool> IncrementVersion(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context);
        Task<bool> InsertToSearchEngine(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context);
        Task<bool> SelectExistingContent(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context);
        Task<bool> UpdateContentDb(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context);
        Task<bool> ValidateInput(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context);
    }
}