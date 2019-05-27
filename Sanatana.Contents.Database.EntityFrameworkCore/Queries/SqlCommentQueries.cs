using Sanatana.Contents.Database.EntityFrameworkCore.Context;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch.Commands.Merge;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Sanatana.EntityFrameworkCore;
using Sanatana.Contents.Objects.DTOs;
using Sanatana.Contents.Utilities;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Objects;
using Sanatana.EntityFrameworkCore.Batch;

namespace Sanatana.Contents.Database.EntityFrameworkCore.Queries
{
    public class SqlCommentQueries<TContent, TComment> : ICommentQueries<long, TContent, TComment>
        where TContent : Content<long>
        where TComment : Comment<long>
    {
        //fields
        protected IContentsDbContextFactory _dbContextFactory;


        //init
        public SqlCommentQueries(IContentsDbContextFactory dbContextFactory)
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
                    expr.ForMember(x => x.FullText, opts => opts.Ignore());
                }
                else if (dataAmmount == DataAmount.DescriptionOnly)
                {
                    expr.ForMember(x => x.FullText, opts => opts.Ignore());
                    expr.ForMember(x => x.ShortText, opts => opts.Ignore());
                }

                cfg.CreateMap<CommentJoinResult<long, TComment, TContent>, CommentJoinResult<long, TComment, TContent>>()
                    .ForMember(dest => dest.Content, x => x.MapFrom(src => src.Content));
            });

            return configuration;
        }


        //methods
        public async Task InsertMany(IEnumerable<TComment> comments)
        {
            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                InsertCommand<TComment> command = repository.Insert<TComment>();
                command.Insert.ExcludeProperty(x => x.CommentId);
                int changes = await command
                    .ExecuteAsync(comments.ToList())
                    .ConfigureAwait(false);
            }
        }

        public async Task<long> Count(Expression<Func<TComment, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            filterConditions = (Expression<Func<TComment, bool>>)visitor.Visit(filterConditions);

            using (ContentsDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                return await dbContext.Set<TComment>()
                    .LongCountAsync(filterConditions)
                    .ConfigureAwait(false);
            }
        }

        public async Task<List<TComment>> SelectMany(int page, int pageSize, bool orderDescending
            , Expression<Func<TComment, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            filterConditions = (Expression<Func<TComment, bool>>)visitor.Visit(filterConditions);

            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                RepositoryResult<TComment> result = await repository
                    .SelectPageAsync(page, pageSize, orderDescending
                        , filterConditions, x => x.CreatedTimeUtc, false, false)
                    .ConfigureAwait(false);

                return result.Data;
            }
        }
               
        public async Task<List<CommentJoinResult<long, TComment, TContent>>> SelectManyJoinedContent(
           int page, int pageSize, bool orderDescending, DataAmount contentDataAmmount
           , Expression<Func<CommentJoinResult<long, TComment, TContent>, bool>> contentFilter)
        {
            int skip = SqlUtility.ToSkipNumberOneBased(page, pageSize);

            var visitor = new EqualityExpressionVisitor();
            contentFilter = (Expression<Func<CommentJoinResult<long, TComment, TContent>, bool>>)visitor.Visit(contentFilter);

            IConfigurationProvider projection = GetProjection(contentDataAmmount);  //map to same type and exclude some props
            
            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                IQueryable<TComment> commentSet = dbContext.Set<TComment>();
                IQueryable<TContent> contentSet = dbContext.Set<TContent>();
                
                IQueryable<CommentJoinResult<long, TComment, TContent>> query = 
                    from c in commentSet
                    join t in contentSet on c.ContentId equals t.ContentId
                    select new CommentJoinResult<long, TComment, TContent>
                    {
                        Comment = c,
                        Content = t
                    };

                if (orderDescending)
                {
                    query = query.OrderByDescending(x => x.Comment.CreatedTimeUtc);
                }
                else
                {
                    query = query.OrderBy(x => x.Comment.CreatedTimeUtc);
                }

                List<CommentJoinResult<long, TComment, TContent>> dtos = await query
                    .Where(contentFilter)
                    .ProjectTo<CommentJoinResult<long, TComment, TContent>>(projection)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync()
                    .ConfigureAwait(false);
                return dtos;
            }
        }
        
        public async Task<TComment> SelectOne(Expression<Func<TComment, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            filterConditions = (Expression<Func<TComment, bool>>)visitor.Visit(filterConditions);

            using (ContentsDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                return await dbContext.Set<TComment>()
                    .Where(filterConditions)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);
            }
        }

        public async Task<long> UpdateMany(IEnumerable<TComment> comments
            , params Expression<Func<TComment, object>>[] propertiesToUpdate)
        {
            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                MergeCommand<TComment> merge = repository.Merge(comments.ToList());
                merge.Compare
                    .IncludeProperty(x => x.CommentId);

                foreach (Expression<Func<TComment, object>> updateProp in propertiesToUpdate)
                {
                    merge.UpdateMatched.IncludeProperty(updateProp);
                }

                return await merge.ExecuteAsync(MergeType.Update)
                    .ConfigureAwait(false);
            }
        }

        public async Task<long> DeleteMany(IEnumerable<TComment> comments)
        {
            IEnumerable<long> ids = comments.Select(x => x.CommentId);

            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                return await repository
                    .DeleteManyAsync<TComment>(x => ids.Contains(x.CommentId))
                    .ConfigureAwait(false);
            }
        }

        public async Task<long> DeleteMany(Expression<Func<TComment, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            filterConditions = (Expression<Func<TComment, bool>>)visitor.Visit(filterConditions);

            ContentsDbContext dbContext = _dbContextFactory.GetDbContext();
            using (Repository repository = new Repository(dbContext))
            {
                return await repository
                    .DeleteManyAsync<TComment>(filterConditions)
                    .ConfigureAwait(false);
            }
        }

    }
}
