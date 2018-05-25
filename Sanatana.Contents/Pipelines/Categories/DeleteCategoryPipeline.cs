using Sanatana.Contents.Database;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Pipelines.Comments;
using Sanatana.Contents.Search;
using Sanatana.Contents.Utilities;
using Sanatana.Patterns.Pipelines;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Sanatana.Contents.Pipelines.Categories
{
    public class DeleteCategoryPipeline<TKey, TCategory> 
        : CategoryPipelineBase<TKey, TCategory>
        where TKey : struct
        where TCategory : Category<TKey>, new()
    {

        //init
        public DeleteCategoryPipeline(IPipelineExceptionHandler exceptionHandler
            , ICategoryQueries<TKey, TCategory> categoryQueries, ISearchQueries<TKey> searchQueries
            , IUrlEncoder urlEncoder)
            : base(exceptionHandler, categoryQueries, searchQueries, urlEncoder)
        {
            RegisterModules();
        }


        //bootstrap
        protected virtual void RegisterModules()
        {
            Register(GetStoredCategory);
            Register(IncrementVersion);
            Register(DeleteCategorySearch);
            Register(DeleteCategoryDb);
        }

        public Task<PipelineResult> ExecuteDelete(TKey categoryId, long version)
        {
            TCategory category = new TCategory()
            {
                   CategoryId = categoryId,
                   Version = version
            };
            var editParams = new CategoryEditParams<TKey, TCategory>()
            {
                Category = category,
                MaxNameLength = 0
            };

            return base.Execute(editParams, null);
        }


        //modules
        public virtual async Task<bool> DeleteCategorySearch(
            PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context)
        {
            TKey categoryId = context.Input.Category.CategoryId;
            if (_storedCategory.IsIndexed == false)
            {
                return true;
            }

            var category = new TCategory
            {
                CategoryId = categoryId,
                Version = context.Input.Category.Version
            };
            await _searchQueries.Delete(new object[] { category }.ToList()).ConfigureAwait(false);

            return true;
        }

        public virtual async Task<bool> DeleteCategoryDb(
            PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context)
        {
            TKey categoryId = context.Input.Category.CategoryId;
            await _categoryQueries
                .DeleteMany(x => EqualityComparer<TKey>.Default.Equals(x.CategoryId, categoryId));

            return true;
        }

    }
}
