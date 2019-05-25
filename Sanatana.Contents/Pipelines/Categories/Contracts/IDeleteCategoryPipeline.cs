using System.Threading.Tasks;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Patterns.Pipelines;

namespace Sanatana.Contents.Pipelines.Categories
{
    public interface IDeleteCategoryPipeline<TKey, TCategory> 
        : ICategoryPipelineBase<TKey, TCategory>
        where TKey : struct
        where TCategory : Category<TKey>, new()
    {
        Task<bool> DeleteCategoryDb(PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context);
        Task<bool> DeleteCategorySearch(PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context);
        Task<PipelineResult> ExecuteDelete(TKey categoryId, long version);
    }
}