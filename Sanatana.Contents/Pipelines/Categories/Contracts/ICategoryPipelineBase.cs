using System.Threading.Tasks;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Patterns.Pipelines;

namespace Sanatana.Contents.Pipelines.Categories
{
    public interface ICategoryPipelineBase<TKey, TCategory> 
        : IPipeline<CategoryEditParams<TKey, TCategory>, PipelineResult>
        where TKey : struct
        where TCategory : Category<TKey>
    {
        Task<bool> CreateUrl(PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context);
        Task<bool> GetStoredCategory(PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context);
        Task<bool> IncrementVersion(PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context);
        Task<bool> SanitizeTitle(PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context);
        Task<bool> UpdateCategorySearch(PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context);
        Task<bool> ValidateInput(PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context);
    }
}