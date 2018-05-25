using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Objects.DTOs;
using Sanatana.Contents.Objects;

namespace Sanatana.Contents.Database
{
    public interface ICommentQueries<TKey, TContent, TComment>
        where TKey : struct
        where TContent : Content<TKey>
        where TComment : Comment<TKey>
    {
        Task InsertMany(IEnumerable<TComment> comments);
        Task<long> Count(Expression<Func<TComment, bool>> filterConditions);
        Task<List<TComment>> SelectMany(int page, int pageSize, bool orderDescending,
            Expression<Func<TComment, bool>> filterConditions);
        Task<List<CommentJoinResult<TKey, TComment, TContent>>> SelectManyJoinedContent(
           int page, int pageSize, bool orderDescending, DataAmount contentDataAmmount
           , Expression<Func<CommentJoinResult<TKey, TComment, TContent>, bool>> filterConditions);
        Task<TComment> SelectOne(
            Expression<Func<TComment, bool>> filterConditions);
        Task<long> UpdateMany(IEnumerable<TComment> comments
            , params Expression<Func<TComment, object>>[] propertiesToUpdate);
        Task<long> DeleteMany(IEnumerable<TComment> comments);
        Task<long> DeleteMany(Expression<Func<TComment, bool>> filterConditions);
    }
}