using System.Threading.Tasks;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Patterns.Pipelines;

namespace Sanatana.Contents.Pipelines.Categories
{
    public interface IUpdateCategoryPipeline<TKey, TCategory> 
        : ICategoryPipelineBase<TKey, TCategory>
        where TKey : struct
        where TCategory : Category<TKey>
    {
        Task<bool> UpdateCategoryDb(PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context);
    }
}