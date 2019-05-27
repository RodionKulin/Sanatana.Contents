using Sanatana.Contents.Database.EntityFrameworkCore.Context;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;
using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.Commands.Merge;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Utilities;

namespace Sanatana.Contents.Database.EntityFrameworkCore.Queries
{
    public class SqlCategoryQueries<TCategory> : ICategoryQueries<long, TCategory>
        where TCategory : Category<long>
    {
        //fields
        protected IContentsDbContextFactory _dbContextFactory;


        //init
        public SqlCategoryQueries(IContentsDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }


        //methods
        public async Task InsertMany(IEnumerable<TCategory> categories)
        {
            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                InsertCommand<TCategory> command = repository.Insert<TCategory>();
                command.Insert.ExcludeProperty(x => x.CategoryId);
                int changes = await command
                    .ExecuteAsync(categories.ToList())
                    .ConfigureAwait(false);
            }
        }

        public async Task<List<TCategory>> SelectMany(Expression<Func<TCategory, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            filterConditions = (Expression<Func<TCategory, bool>>)visitor.Visit(filterConditions);

            using (ContentsDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                return await dbContext.Set<TCategory>()
                    .Where(filterConditions)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
        }

        public async Task<long> UpdateMany(IEnumerable<TCategory> categories
            , params Expression<Func<TCategory, object>>[] propertiesToUpdate)
        {
            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                MergeCommand<TCategory> merge = repository.Merge(categories.ToList());
                merge.Compare
                    .IncludeProperty(x => x.CategoryId);
                return await merge.ExecuteAsync(MergeType.Update)
                    .ConfigureAwait(false);
            }
        }
        
        public async Task<long> DeleteMany(Expression<Func<TCategory, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            filterConditions = (Expression<Func<TCategory, bool>>)visitor.Visit(filterConditions);

            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                return await repository
                    .DeleteManyAsync<TCategory>(filterConditions)
                    .ConfigureAwait(false);
            }
        }

    }
}
