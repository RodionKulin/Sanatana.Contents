using Sanatana.Contents.Database.EntityFrameworkCore.Context;
using Sanatana.EntityFrameworkCore.Commands;
using Sanatana.EntityFrameworkCore.Commands.Merge;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Utilities;

namespace Sanatana.Contents.Database.EntityFrameworkCore.Queries
{
    public class SqlCategoryRolePermissionQueries : ICategoryRolePermissionQueries<long>
    {
        //fields
        protected IContentsDbContextFactory _dbContextFactory;


        //init
        public SqlCategoryRolePermissionQueries(IContentsDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }


        //methods
        public async Task InsertMany(IEnumerable<CategoryRolePermission<long>> categoryRolePermissions)
        {
            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                InsertCommand<CategoryRolePermission<long>> command = repository.Insert<CategoryRolePermission<long>>();
                command.Insert.ExcludeProperty(x => x.CategoryId);
                int changes = await command
                    .ExecuteAsync(categoryRolePermissions.ToList())
                    .ConfigureAwait(false);
            }
        }

        public async Task<List<CategoryRolePermission<long>>> SelectMany(
            Expression<Func<CategoryRolePermission<long>, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            filterConditions = (Expression<Func<CategoryRolePermission<long>, bool>>)visitor.Visit(filterConditions);

            using (ContentsDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                return await dbContext.Set<CategoryRolePermission<long>>()
                    .Where(filterConditions)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
        }

        public async Task<long> UpdateMany(IEnumerable<CategoryRolePermission<long>> categoryRolePermissions)
        {
            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                MergeCommand<CategoryRolePermission<long>> merge = repository.MergeParameters(categoryRolePermissions.ToList());
                merge.Compare
                    .IncludeProperty(x => x.CategoryRolePermissionId);
                return await merge.ExecuteAsync(MergeType.Update)
                    .ConfigureAwait(false);
            }
        }

        public async Task<long> DeleteMany(Expression<Func<CategoryRolePermission<long>, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            filterConditions = (Expression<Func<CategoryRolePermission<long>, bool>>)visitor.Visit(filterConditions);

            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                return await repository
                    .DeleteManyAsync<CategoryRolePermission<long>>(filterConditions)
                    .ConfigureAwait(false);
            }
        }

    }
}
