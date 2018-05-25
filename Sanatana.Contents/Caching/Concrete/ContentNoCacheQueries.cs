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
    public class ContentNoCacheQueries<TKey, TContent> : IContentQueries<TKey, TContent>
        where TKey : struct
        where TContent : Content<TKey>
    {
        //fields
        protected IContentQueries<TKey, TContent> _queries;

        
        //init
        public ContentNoCacheQueries(IContentQueries<TKey, TContent> queries)
        {
            _queries = queries;
        }

        
        //Insert
        public virtual Task<ContentInsertResult> InsertOne(TContent content)
        {
            return _queries.InsertOne(content);
        }

        public virtual Task InsertMany(IEnumerable<TContent> contents)
        {
            return _queries.InsertMany(contents);
        }



        //Select
        public virtual Task<List<TContent>> SelectTopViews(int pageSize
            , DataAmount dataAmmount, Expression<Func<TContent, bool>> filterConditions)
        {
            return _queries.SelectTopViews(pageSize, dataAmmount, filterConditions);
        }

        public virtual Task<List<ContentCategoryGroupResult<TKey, TContent>>> SelectLatestFromEachCategory(
            int eachCategoryCount, DataAmount dataAmmount, Expression<Func<TContent, bool>> filterConditions)
        {
            return _queries.SelectLatestFromEachCategory(eachCategoryCount, dataAmmount, filterConditions);
        }

        public virtual Task<TContent> SelectOne(bool incrementViewCount, DataAmount dataAmmount
            , Expression<Func<TContent, bool>> filterConditions)
        {
            return _queries.SelectOne(incrementViewCount, dataAmmount, filterConditions);
        }

        public virtual Task<List<TContent>> SelectMany(
            int page, int pageSize, DataAmount dataAmmount, bool orderDescending
            , Expression<Func<TContent, bool>> filterConditions)
        {
            return _queries.SelectMany(page, pageSize, dataAmmount, orderDescending, filterConditions);
        }
        

        //Count
        public virtual Task<long> Count(Expression<Func<TContent, bool>> filterConditions)
        {
            return _queries.Count(filterConditions);
        }



        //Update
        public virtual Task<long> IncrementCommentsCount(int increment, Expression<Func<TContent, bool>> filterConditions)
        {
            return _queries.IncrementCommentsCount(increment, filterConditions);
        }

        public virtual Task<long> UpdateMany(TContent values
            , Expression<Func<TContent, bool>> filterConditions
            , params Expression<Func<TContent, object>>[] propertiesToUpdate)
        {
            return _queries.UpdateMany(values, filterConditions, propertiesToUpdate);
        }

        public virtual Task<OperationStatus> UpdateOne(TContent content
            , long prevVersion, bool matchVersion
            , params Expression<Func<TContent, object>>[] propertiesToUpdate)
        {
            return _queries.UpdateOne(content, prevVersion, matchVersion, propertiesToUpdate);
        }

        public virtual Task<OperationStatus> UpdateOne(TContent content
            , long prevVersion, bool matchVersion)
        {
            return _queries.UpdateOne(content, prevVersion, matchVersion);
        }


        //Delete
        public virtual Task<long> DeleteMany(Expression<Func<TContent, bool>> filterConditions)
        {
            return _queries.DeleteMany(filterConditions);
        }



    }
}
