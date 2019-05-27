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
    public class CommentNoCacheQueries<TKey, TContent, TComment> 
        : ICommentQueries<TKey, TContent, TComment>
        where TKey : struct
        where TContent : Content<TKey>
        where TComment : Comment<TKey>
    {
        //fields
        protected ICommentQueries<TKey, TContent, TComment> _queries;


        //init
        public CommentNoCacheQueries(ICommentQueries<TKey, TContent, TComment> queries)
        {
            _queries = queries;
        }

        
        //methods
        public virtual Task InsertMany(IEnumerable<TComment> comments)
        {
            return _queries.InsertMany(comments);
        }
        
        public virtual Task<List<TComment>> SelectMany(int page, int pageSize, bool orderDescending
            , Expression<Func<TComment, bool>> filterConditions)
        {
            return _queries.SelectMany(page, pageSize, orderDescending, filterConditions);
        }

        public virtual Task<List<CommentJoinResult<TKey, TComment, TContent>>> SelectManyJoinedContent(
            int page, int pageSize, bool orderDescending, DataAmount contentDataAmmount
           , Expression<Func<CommentJoinResult<TKey, TComment, TContent>, bool>> filterConditions)
        {
            return _queries.SelectManyJoinedContent(page, pageSize, orderDescending, contentDataAmmount
                , filterConditions);
        }

        public virtual Task<TComment> SelectOne(Expression<Func<TComment, bool>> filterConditions)
        {
            return _queries.SelectOne(filterConditions);
        }

        public virtual Task<long> UpdateMany(IEnumerable<TComment> comments
            , params Expression<Func<TComment, object>>[] propertiesToUpdate)
        {
            return _queries.UpdateMany(comments, propertiesToUpdate);
        }

        public virtual Task<long> DeleteMany(Expression<Func<TComment, bool>> filterConditions)
        {
            return _queries.DeleteMany(filterConditions);
        }

        public virtual Task<long> DeleteMany(IEnumerable<TComment> comments)
        {
            return _queries.DeleteMany(comments);
        }


        public Task<long> Count(Expression<Func<TComment, bool>> filterConditions)
        {
            return _queries.Count(filterConditions);
        }
        
    }
}
