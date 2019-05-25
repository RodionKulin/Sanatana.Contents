using Sanatana.Contents.Database;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Search;
using Sanatana.Contents.Utilities;
using Sanatana.Patterns.Pipelines;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Pipelines.Categories
{
    public class UpdateCategoryPipeline<TKey, TCategory>
        : CategoryPipelineBase<TKey, TCategory>, 
        IUpdateCategoryPipeline<TKey, TCategory> 
        where TKey : struct
        where TCategory : Category<TKey>
    {
        //init
        public UpdateCategoryPipeline(IPipelineExceptionHandler exceptionHandler
            , ICategoryQueries<TKey, TCategory> categoryQueries, ISearchQueries<TKey> searchQueries
            , IUrlEncoder urlEncoder)
            : base(exceptionHandler, categoryQueries, searchQueries, urlEncoder)
        {
            RegisterModules();
        }


        //bootstrap
        protected virtual void RegisterModules()
        {
            Register(ValidateInput);
            Register(SanitizeTitle);
            Register(CreateUrl);
            Register(IncrementVersion);
            Register(UpdateCategorySearch);
            Register(UpdateCategoryDb);
        }


        //modules
        public virtual async Task<bool> UpdateCategoryDb(
            PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context)
        {
            TCategory category = context.Input.Category;

            long updatedItems = await _categoryQueries
                .UpdateMany(new[] { category }, x => x.Name 
                , x => x.IsIndexed
                , x => x.NeverIndex
                , x => x.ParentCategoryId
                , x => x.SortOrder
                , x => x.Url
                , x => x.Version)
                .ConfigureAwait(false);

            return true;
        }


    }
}
