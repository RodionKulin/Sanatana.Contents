using Sanatana.Contents.Database;
using Sanatana.Contents.Html;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Resources;
using Sanatana.Contents.Search;
using Sanatana.Contents.Utilities;
using Sanatana.Patterns.Pipelines;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Pipelines.Categories
{
    public class InsertCategoryPipeline<TKey, TCategory> : CategoryPipelineBase<TKey, TCategory>
        where TKey : struct
        where TCategory : Category<TKey>
    {
        //init
        public InsertCategoryPipeline(IPipelineExceptionHandler exceptionHandler
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
            Register(UpdateCategorySearch);
            Register(InsertCategoryDb);
        }


        //modules
        public virtual async Task<bool> InsertCategoryDb(
            PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context)
        {
            TCategory category = context.Input.Category;

            await _categoryQueries
                .InsertMany(new []{ category })                
                .ConfigureAwait(false);
            
            return true;
        }


    }
}
