using System.Threading.Tasks;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Patterns.Pipelines;

namespace Sanatana.Contents.Pipelines.Contents
{
    public interface IDeleteContentPipeline<TKey, TCategory, TContent, TComment>
        : IPipeline<ContentDeleteParams<TKey>, ContentUpdateResult>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>, new()
        where TComment : Comment<TKey>
    {
        Task<bool> CheckPermission(PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context);
        Task<bool> CheckVersion(PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context);
        Task<bool> DeleteCommentsDb(PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context);
        Task<bool> DeleteContentDb(PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context);
        Task<bool> DeleteContentImages(PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context);
        Task<bool> DeleteContentSearch(PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context);
        Task<bool> IncrementVersion(PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context);
        Task<bool> SelectExisting(PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context);
    }
}