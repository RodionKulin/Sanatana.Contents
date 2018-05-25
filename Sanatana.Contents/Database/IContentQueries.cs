using Sanatana.Contents.Objects;
using Sanatana.Contents.Objects.DTOs;
using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Database
{
    public interface IContentQueries<TKey, TContent>
        where TKey : struct
        where TContent : Content<TKey>
    {
        Task<ContentInsertResult> InsertOne(TContent content);
        Task InsertMany(IEnumerable<TContent> contents);

        Task<long> Count(Expression<Func<TContent, bool>> filterConditions);
        Task<List<ContentCategoryGroupResult<TKey, TContent>>> SelectLatestFromEachCategory(int eachCategoryCount, DataAmount dataAmmount, Expression<Func<TContent, bool>> filterConditions);
        Task<TContent> SelectOne(bool incrementViewCount, DataAmount dataAmmount, Expression<Func<TContent, bool>> filterConditions);
        Task<List<TContent>> SelectMany(int page, int pageSize, DataAmount dataAmmount, bool orderDescending, Expression<Func<TContent, bool>> filterConditions);
        Task<List<TContent>> SelectTopViews(int pageSize, DataAmount dataAmmount, Expression<Func<TContent, bool>> filterConditions);

        Task<long> IncrementCommentsCount(int increment, Expression<Func<TContent, bool>> filterConditions);
        Task<OperationStatus> UpdateOne(TContent content, long prevVersion, bool matchVersion
            , params Expression<Func<TContent, object>>[] propertiesToUpdate);
        Task<OperationStatus> UpdateOne(TContent content, long prevVersion, bool matchVersion);
        Task<long> UpdateMany(TContent values, Expression<Func<TContent, bool>> filterConditions
            , params Expression<Func<TContent, object>>[] propertiesToUpdate);

        Task<long> DeleteMany(Expression<Func<TContent, bool>> filterConditions);
    }
}
