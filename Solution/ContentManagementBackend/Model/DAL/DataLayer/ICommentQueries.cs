using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Utility;
using System.Linq.Expressions;
using System;

namespace ContentManagementBackend
{
    public interface ICommentQueries<TKey>
        where TKey : struct
    {
        Task<bool> Insert(Comment<TKey> comment);
        Task<QueryResult<List<Comment<TKey>>>> Select(
            List<CommentState> states, int page, int pageSize, TKey? contentID = null);
        Task<QueryResult<bool>> UpdateContent(
            Comment<TKey> comment, bool matchAuthorID
            , params Expression<Func<Comment<TKey>, object>>[] fieldsToUpdate);
        Task<bool> Delete(Comment<TKey> comment);
        Task<bool> Delete(TKey contentID);
    }
}