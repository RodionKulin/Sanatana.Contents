using Sanatana.Contents.Database.EntityFrameworkCore.Context;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using Sanatana.Contents.Objects;
using Sanatana.Contents.Objects.DTOs;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Utilities;
using Sanatana.EntityFrameworkCore.Batch.Commands;

namespace Sanatana.Contents.Database.EntityFrameworkCore.Queries
{
    public class SqlContentQueries<TContent> : IContentQueries<long, TContent>
        where TContent : Content<long>
    {
        //fields
        protected IContentsDbContextFactory _dbContextFactory;


        //init
        public SqlContentQueries(IContentsDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }


        //common
        protected virtual IConfigurationProvider GetProjection(DataAmount dataAmmount)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                IMappingExpression<TContent, TContent> expr = cfg.CreateMap<TContent, TContent>();

                if (dataAmmount == DataAmount.ShortContent)
                {
                    expr.ForMember(x => x.FullText, mem => mem.Ignore());
                }
                else if (dataAmmount == DataAmount.DescriptionOnly)
                {
                    expr.ForMember(x => x.FullText, mem => mem.Ignore());
                    expr.ForMember(x => x.ShortText, mem => mem.Ignore());
                }
            });

            return configuration;
        }


        //insert
        public async Task<ContentInsertResult> InsertOne(TContent content)
        {
            using (ContentsDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                List<TContent> matchingContent = await dbContext.Set<TContent>()
                    .Where(x => x.Url == content.Url
                        || x.PublishedTimeUtc == content.PublishedTimeUtc)
                    .ToListAsync()
                    .ConfigureAwait(false);

                if(matchingContent.Any(x => x.Url == content.Url))
                {
                    return ContentInsertResult.UrlIsNotUnique;
                }
                else if (matchingContent.Any(x => x.PublishedTimeUtc == content.PublishedTimeUtc))
                {
                    return ContentInsertResult.PublishTimeUtcIsNotUnique;
                }
                    
                dbContext.Add(content);
                int changes = await dbContext.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            return ContentInsertResult.Success;
        }

        public async Task InsertMany(IEnumerable<TContent> contents)
        {
            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                InsertCommand<TContent> command = repository.Insert<TContent>();
                command.Insert.ExcludeProperty(x => x.ContentId);
                command.Output.IncludeProperty(x => x.ContentId);
                int changes = await command
                    .ExecuteAsync(contents.ToList())
                    .ConfigureAwait(false);
            }
        }
        

        //select
        public async Task<List<ContentCategoryGroupResult<long, TContent>>> SelectLatestFromEachCategory(
            int eachCategoryCount, DataAmount dataAmmount, Expression<Func<TContent, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            filterConditions = (Expression<Func<TContent, bool>>)visitor.Visit(filterConditions);

            IConfigurationProvider projection = GetProjection(dataAmmount);

            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                return await dbContext
                    .Set<TContent>()
                    .Where(filterConditions)
                    .ProjectTo<TContent>(projection)
                    .GroupBy(x => x.CategoryId)
                    .Select(x => new ContentCategoryGroupResult<long, TContent>
                    {
                        CategoryId = x.Key,
                        Contents = x.Take(eachCategoryCount).ToList()
                    })
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
        }

        public async Task<List<TContent>> SelectMany(int page, int pageSize, DataAmount dataAmmount
            , bool orderDescending, Expression<Func<TContent, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            filterConditions = (Expression<Func<TContent, bool>>)visitor.Visit(filterConditions);

            IConfigurationProvider projection = GetProjection(dataAmmount);

            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                return await repository
                    .SelectPageQuery(page, pageSize, orderDescending, filterConditions, x => x.PublishedTimeUtc, false)
                    .ProjectTo<TContent>(projection)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
        }

        public async Task<TContent> SelectOne(bool incrementViewCount, DataAmount dataAmmount
            , Expression<Func<TContent, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            filterConditions = (Expression<Func<TContent, bool>>)visitor.Visit(filterConditions);

            IConfigurationProvider projection = GetProjection(dataAmmount);

            ContentsDbContext updateContext = _dbContextFactory.GetDbContext();
            using (Repository updateRepository = new Repository(updateContext))
            using (ContentsDbContext findContext = _dbContextFactory.GetDbContext())
            {
                UpdateCommand<TContent> updateCommand = updateRepository.UpdateMany(filterConditions);
                updateCommand.Assign(x => x.CommentsCount, x => x.ViewsCount + 1);
                Task<int> updateTask = updateCommand.ExecuteAsync();

                Task<TContent> findTask = findContext.Set<TContent>()
                    .Where(filterConditions)
                    .ProjectTo<TContent>(projection)
                    .FirstOrDefaultAsync();

                int updatedCount = await updateTask.ConfigureAwait(false);
                return await findTask.ConfigureAwait(false);
            }
        }

        public async Task<List<TContent>> SelectTopViews(int pageSize, DataAmount dataAmmount
            , Expression<Func<TContent, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            filterConditions = (Expression<Func<TContent, bool>>)visitor.Visit(filterConditions);

            IConfigurationProvider projection = GetProjection(dataAmmount);

            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                return await repository
                    .SelectPageQuery(1, pageSize, true, filterConditions, x => x.ViewsCount, false)
                    .ProjectTo<TContent>(projection)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
        }

        public async Task<long> Count(Expression<Func<TContent, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            filterConditions = (Expression<Func<TContent, bool>>)visitor.Visit(filterConditions);

            using (ContentsDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                return await dbContext.Set<TContent>()
                    .LongCountAsync(filterConditions)
                    .ConfigureAwait(false);
            }
        }

        public async Task<long> IncrementCommentsCount(int increment
            , Expression<Func<TContent, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            filterConditions = (Expression<Func<TContent, bool>>)visitor.Visit(filterConditions);

            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                UpdateCommand<TContent> updateCommand = repository.UpdateMany(filterConditions);
                updateCommand.Assign(x => x.CommentsCount, x => x.CommentsCount + increment);

                return await updateCommand.ExecuteAsync()
                    .ConfigureAwait(false);
            }
        }


        //update
        public async Task<long> UpdateMany(TContent values, Expression<Func<TContent, bool>> filterConditions
            , params Expression<Func<TContent, object>>[] propertiesToUpdate)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            filterConditions = (Expression<Func<TContent, bool>>)visitor.Visit(filterConditions);

            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                UpdateCommand<TContent> updateCommand = repository.UpdateMany(filterConditions);

                foreach (Expression<Func<TContent, object>> updateProp in propertiesToUpdate)
                {
                    object value = updateProp.Compile().Invoke(values);
                    updateCommand.Assign(updateProp, x => value);
                }

                return await updateCommand.ExecuteAsync()
                    .ConfigureAwait(false);
            }
        }

        public async Task<OperationStatus> UpdateOne(TContent content
            , long prevVersion, bool matchVersion)
        {
            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                if (matchVersion)
                {
                    TContent existingContent = await dbContext.Set<TContent>()
                        .FirstOrDefaultAsync(x => x.ContentId == content.ContentId)
                        .ConfigureAwait(false);

                    if (existingContent == null)
                    {
                        return OperationStatus.NotFound;
                    }
                    if (existingContent.Version != prevVersion)
                    {
                        return OperationStatus.VersionChanged;
                    }
                }

                int updateCount = await repository
                     .UpdateOneAsync(content)
                     .ConfigureAwait(false);
                if (updateCount == 0)
                {
                    return OperationStatus.NotFound;
                }
            }

            return OperationStatus.Success;
        }

        public async Task<OperationStatus> UpdateOne(TContent content, long prevVersion, bool matchVersion
            , params Expression<Func<TContent, object>>[] propertiesToUpdate)
        {
            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                if (matchVersion)
                {
                    TContent existingContent = await dbContext.Set<TContent>()
                        .FirstOrDefaultAsync(x => x.ContentId == content.ContentId)
                        .ConfigureAwait(false);

                    if (existingContent == null)
                    {
                        return OperationStatus.NotFound;
                    }
                    if (existingContent.Version != prevVersion)
                    {
                        return OperationStatus.VersionChanged;
                    }
                }

                int updateCount = await repository
                     .UpdateOneAsync(content, propertiesToUpdate)
                     .ConfigureAwait(false);
                if (updateCount == 0)
                {
                    return OperationStatus.NotFound;
                }
            }

            return OperationStatus.Success;
        }


        //delete
        public async Task<long> DeleteMany(Expression<Func<TContent, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            filterConditions = (Expression<Func<TContent, bool>>)visitor.Visit(filterConditions);

            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                return await repository
                    .DeleteManyAsync<TContent>(filterConditions)
                    .ConfigureAwait(false);
            }
        }

    }
}
