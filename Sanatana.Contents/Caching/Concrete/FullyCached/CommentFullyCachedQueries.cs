using Sanatana.Contents.Caching.CacheProviders;
using Sanatana.Contents.Database;
using Sanatana.Contents.Objects;
using Sanatana.Contents.Objects.DTOs;
using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Caching.Concrete
{
    public class CommentFullyCachedQueries<TKey, TContent, TComment> 
        : ICommentQueries<TKey, TContent, TComment>
        where TKey : struct
        where TContent : Content<TKey>
        where TComment : Comment<TKey>
    {
        //fields
        protected ICommentQueries<TKey, TContent, TComment> _queries;
        protected IQueryCache _queryCache;
        protected ICacheProvider _cacheProvider;


        //init
        public CommentFullyCachedQueries(ICommentQueries<TKey, TContent, TComment> queries
            , IQueryCache queryCache, ICacheProvider cacheProvider)
        {
            _queries = queries;
            _queryCache = queryCache;
            _cacheProvider = cacheProvider;
        }


        //common
        protected virtual async Task<List<TComment>> SelectAll()
        {
            string cacheKey = UrnId.Create<List<TComment>>();

            List<TComment> list = await _queryCache
                .ToOptimizedResultUsingCache(cacheKey, () =>
                {
                    return _queries.SelectMany(1, int.MaxValue, true, x => true);
                }
                , ContentsConstants.DEFAULT_CACHE_EXPIRY_PERIOD
                , _queryCache.GetDataChangeNotifier<TComment>())
                .ConfigureAwait(false);

            return new List<TComment>(list);
        }


        //methods
        public virtual async Task InsertMany(IEnumerable<TComment> comments)
        {
            await  _queries.InsertMany(comments).ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<TComment>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
        }
        
        public virtual async Task<List<TComment>> SelectMany(int page, int pageSize, bool orderDescending
            , Expression<Func<TComment, bool>> filterConditions)
        {
            IEnumerable<TComment> allComments = await SelectAll().ConfigureAwait(false);
            allComments = allComments.Where(filterConditions.Compile());

            int skip = (page - 1) * pageSize;
            IEnumerable<TComment> query = allComments.Skip(skip).Take(pageSize);
            if (orderDescending)
            {
                query = query.OrderByDescending(x => x.CreatedTimeUtc);
            }
            else
            {
                query = query.OrderBy(x => x.CreatedTimeUtc);
            }
                
            return query.ToList();
        }

        public virtual Task<List<CommentJoinResult<TKey, TComment, TContent>>> SelectManyJoinedContent(
            int page, int pageSize, bool orderDescending, DataAmount contentDataAmmount
           , Expression<Func<CommentJoinResult<TKey, TComment, TContent>, bool>> filterConditions)
        {
            return _queries.SelectManyJoinedContent(page, pageSize, orderDescending, contentDataAmmount
                , filterConditions);
        }

        public virtual async Task<TComment> SelectOne(
            Expression<Func<TComment, bool>> filterConditions)
        {
            IEnumerable<TComment> allComments = await SelectAll().ConfigureAwait(false);
            allComments = allComments.Where(filterConditions.Compile());

            return allComments.FirstOrDefault();
        }

        public virtual async Task<long> UpdateMany(IEnumerable<TComment> comments
            , params Expression<Func<TComment, object>>[] propertiesToUpdate)
        {
            long result = await _queries.UpdateMany(comments, propertiesToUpdate).ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<TComment>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
            return result;
        }

        public virtual async Task<long> DeleteMany(Expression<Func<TComment, bool>> filterConditions)
        {
            long result = await _queries.DeleteMany(filterConditions).ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<TComment>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
            return result;
        }

        public virtual async Task<long> DeleteMany(IEnumerable<TComment> comments)
        {
            long result = await _queries.DeleteMany(comments).ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<TComment>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
            return result;
        }


        public async Task<long> Count(Expression<Func<TComment, bool>> filterConditions)
        {
            IEnumerable<TComment> allComments = await SelectAll().ConfigureAwait(false);
            
            return allComments.Count(filterConditions.Compile());
        }
        
    }
}
