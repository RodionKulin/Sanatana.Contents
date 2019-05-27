using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using LinqKit;
using Sanatana.Contents.Database;
using Sanatana.Contents.Objects;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Search;
using Sanatana.Contents.Selectors.Categories;

namespace Sanatana.Contents.RegularJobs
{
    /// <summary>
    /// Will index in search engine all content that reached it's PublishDateUtc and was not indexed yet.
    /// Implementation description: This job follows logic of default InsertContentPipeline and UpdateContentPipeline.
    /// if content passed through it has future PublishDateUtc, than it will not be indexed.
    /// This job also does not check all the previously created content each time when executed.
    /// Job will remember it's last execution time and check content only after this time.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class IndexFutureContentJob<TKey, TContent> : IContentRegularJob
        where TKey : struct
        where TContent : Content<TKey>, new()
    {
        //fields
        protected IContentQueries<TKey, TContent> _contentQueries;
        protected ISearchQueries<TKey> _searchQueries;
        protected DateTime _lastRequestTimeUtc;
        protected IndexFutureContentSettings<TKey, TContent> _settings;


        //init
        public IndexFutureContentJob(IndexFutureContentSettings<TKey, TContent> settings, 
            IContentQueries<TKey, TContent> contentQueries,
            ISearchQueries<TKey> searchQueries)
        {
            _settings = settings;
            _contentQueries = contentQueries;
            _searchQueries = searchQueries;

            _lastRequestTimeUtc = DateTime.MinValue;
        }


        //methods
        public virtual void Execute()
        {
            List<TContent> notIndexedContent = null;

            do
            {
                Expression<Func<TContent, bool>> filter = 
                    x => x.NeverIndex == false
                    && x.IsIndexed == false
                    && x.PublishedTimeUtc <= DateTime.UtcNow
                    && x.PublishedTimeUtc >= _lastRequestTimeUtc;
                if(_settings.AdditionalFilters != null)
                {
                    filter = filter.And(_settings.AdditionalFilters);
                }
                notIndexedContent = _contentQueries.SelectMany(
                    1, _settings.PageSize, DataAmount.FullContent, true, filter)
                    .Result;
                if (notIndexedContent.Count == 0)
                {
                    break;
                }

                List<object> searchList = notIndexedContent.Cast<object>().ToList();
                _searchQueries.Insert(searchList).Wait();

                List<TKey> notIndexContentIds = notIndexedContent
                    .Select(p => p.ContentId)
                    .ToList();

                var updatedValues = new TContent()
                {
                    IsIndexed = true
                };
                long updatedCount = _contentQueries.UpdateMany(updatedValues,
                    x => notIndexContentIds.Contains(x.ContentId),
                    new Expression<Func<TContent, object>>[] { c => c.IsIndexed })
                    .Result;
            }
            while (notIndexedContent.Count == _settings.PageSize);

            _lastRequestTimeUtc = DateTime.UtcNow;
        }

    }
}
