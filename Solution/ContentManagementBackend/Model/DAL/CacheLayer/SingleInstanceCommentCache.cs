using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Common.Utility;

namespace ContentManagementBackend
{
    public class SingleInstanceCommentCache<TKey> : ICommentQueries<TKey>
        where TKey : struct
    {
        //поля
        protected ICommentQueries<TKey> _queries;
        protected ICacheProvider _cache;


        //свойства
        protected virtual QueryResult<List<Comment<TKey>>> Latest
        {
            get
            {
                return (QueryResult<List<Comment<TKey>>>)
                    _cache.Get(Constants.CACHE_LATEST_COMMENTS_KEY);
            }
            set
            {
                _cache.Set(Constants.CACHE_LATEST_COMMENTS_KEY, value);
            }
        }



        //инициализация
        public SingleInstanceCommentCache(ICommentQueries<TKey> queries, ICacheProvider cache)
        {
            _queries = queries;
            _cache = cache;
        }



        //методы
        public async Task<bool> Insert(Comment<TKey> comment)
        {
            bool result = await _queries.Insert(comment);

            bool isPublic = comment.State != CommentState.AuthorizationRequired
                && comment.State != CommentState.AuthorizationRequiredModerated;

            if (result && isPublic)
            {
                _cache.Remove(Constants.CACHE_LATEST_COMMENTS_KEY);
            }

            return result;
        }
        
        public async Task<QueryResult<List<Comment<TKey>>>> Select(List<CommentState> states
            , int page, int pageSize, TKey? contentID = null)
        {
            bool isLatestQuery = contentID == null
                && page == 1
                && !states.Contains(CommentState.AuthorizationRequired)
                && !states.Contains(CommentState.AuthorizationRequiredModerated);

            if (isLatestQuery && Latest != null)
            {
                return Latest;
            }

            QueryResult<List<Comment<TKey>>> result = 
                await _queries.Select(states, page, pageSize, contentID);

            if(isLatestQuery && !result.HasExceptions)
            {
                Latest = result;
            }

            return result;
        }

        public async Task<QueryResult<bool>> UpdateContent(
            Comment<TKey> comment, bool matchAuthorID
            , params Expression<Func<Comment<TKey>, object>>[] fieldsToUpdate)
        {
            QueryResult<bool> result = 
                await _queries.UpdateContent(comment, matchAuthorID, fieldsToUpdate);

            if (!result.HasExceptions
                && result.Result
               && Latest != null
               && Latest.Result.Any(p => EqualityComparer<TKey>.Default.Equals(p.CommentID, comment.CommentID)))
            {
                _cache.Remove(Constants.CACHE_LATEST_COMMENTS_KEY);
            }

            return result;
        }

        public async Task<bool> Delete(TKey contentID)
        {
            bool result = await _queries.Delete(contentID);

            if (result
                && Latest != null
                && Latest.Result.Any(p => EqualityComparer<TKey>.Default.Equals(p.ContentID, contentID)))
            {
                _cache.Remove(Constants.CACHE_LATEST_COMMENTS_KEY);
            }

            return result;
        }

        public async Task<bool> Delete(Comment<TKey> comment)
        {
            bool result = await _queries.Delete(comment);

            if(result 
                && Latest != null 
                && Latest.Result.Any(p => EqualityComparer<TKey>.Default.Equals(p.CommentID, comment.CommentID)))
            {
                _cache.Remove(Constants.CACHE_LATEST_COMMENTS_KEY);
            }

            return result;
        }


    }
}
