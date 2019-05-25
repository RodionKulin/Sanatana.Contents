using System.Threading.Tasks;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Patterns.Pipelines;

namespace Sanatana.Contents.Pipelines.Contents
{
    public interface IContentPipelineBase<TKey, TCategory, TContent>
        : IPipeline<ContentUpdateParams<TKey, TContent>, ContentUpdateResult>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
    {
        Task<bool> CheckPermission(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context);
        Task<bool> CreateShortContent(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context);
        Task<bool> CreateUrl(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context);
        Task<bool> FixIframeSrc(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context);
        Task<bool> InsertContentDb(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context);
        Task<bool> ReplaceContentTempImagesSrc(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context);
        Task<bool> SanitizeFullContent(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context);
        Task<bool> SanitizeShortContent(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context);
        Task<bool> SanitizeTitle(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context);
        Task<bool> UpdateContentSearch(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context);
    }
}