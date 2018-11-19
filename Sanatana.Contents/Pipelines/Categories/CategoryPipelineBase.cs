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
using System.Linq;

namespace Sanatana.Contents.Pipelines.Categories
{
    public class CategoryPipelineBase<TKey, TCategory>
        : Pipeline<CategoryEditParams<TKey, TCategory>>
        where TKey : struct
        where TCategory : Category<TKey>
    {
        //fields
        protected IPipelineExceptionHandler _exceptionHandler;
        protected ICategoryQueries<TKey, TCategory> _categoryQueries;
        protected ISearchQueries<TKey> _searchQueries;
        protected IUrlEncoder _urlEncoder;
        protected TCategory _storedCategory;


        //init
        public CategoryPipelineBase(IPipelineExceptionHandler exceptionHandler
            , ICategoryQueries<TKey, TCategory> categoryQueries, ISearchQueries<TKey> searchQueries
            , IUrlEncoder urlEncoder)
        {
            _exceptionHandler = exceptionHandler;
            _categoryQueries = categoryQueries;
            _searchQueries = searchQueries;
            _urlEncoder = urlEncoder;
        }


        //bootstrap
        public override Task<PipelineResult> Execute(CategoryEditParams<TKey, TCategory> inputModel, PipelineResult outputModel)
        {
            outputModel = outputModel ?? PipelineResult.Success();
            return base.Execute(inputModel, outputModel);
        }

        public override async Task RollBack(PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context)
        {
            await base.RollBack(context);

            if (context.Exceptions != null)
            {
                _exceptionHandler.Handle(context);
            }
            if (context.Output == null)
            {
                context.Output = PipelineResult.Error(ContentsMessages.Common_ProcessingError);
            }

            return;
        }


        //modules
        public virtual Task<bool> ValidateInput(
            PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context)
        {
            TCategory category = context.Input.Category;

            if (string.IsNullOrEmpty(category.Name))
            {
                context.Output = PipelineResult.Error(ContentsMessages.Content_TitleEmpty);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public virtual Task<bool> SanitizeTitle(
            PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context)
        {
            TCategory category = context.Input.Category;

            category.Name = HtmlTagRemover.StripHtml(category.Name);

            if (category.Name.Length > context.Input.MaxNameLength)
                category.Name = category.Name.Substring(0, context.Input.MaxNameLength);

            return Task.FromResult(true);
        }

        public virtual Task<bool> CreateUrl(
            PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context)
        {
            TCategory category = context.Input.Category;

            category.Url = _urlEncoder.Encode(category.Name)
                .ToLowerInvariant();

            return Task.FromResult(true);
        }

        public virtual Task<bool> IncrementVersion(
            PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context)
        {
            TCategory category = context.Input.Category;
            category.Version++;
            return Task.FromResult(true);
        }

        public virtual async Task<bool> GetStoredCategory(
            PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context)
        {
            TKey categoryId = context.Input.Category.CategoryId;
            List<TCategory> categoryList = await _categoryQueries
                .SelectMany(x => EqualityComparer<TKey>.Default.Equals(x.CategoryId, categoryId))
                .ConfigureAwait(false);

            _storedCategory = categoryList.Single();
            return true;
        }

        public virtual async Task<bool> UpdateCategorySearch(
            PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context)
        {
            TCategory category = context.Input.Category;

            bool doIndex = ShouldIndex(context);

            if (doIndex == true)
            {
                await _searchQueries.Insert(new List<object> { category }).ConfigureAwait(false);
            }
            else if (doIndex == false && category.IsIndexed == true)
            {
                await _searchQueries.Delete(new List<object> { category }).ConfigureAwait(false);
            }

            category.IsIndexed = doIndex;
            return true;
        }

        protected bool ShouldIndex(
            PipelineContext<CategoryEditParams<TKey, TCategory>, PipelineResult> context)
        {
            TCategory category = context.Input.Category;

            bool doIndex = category.NeverIndex == false;

            return doIndex;
        }

    }
}
