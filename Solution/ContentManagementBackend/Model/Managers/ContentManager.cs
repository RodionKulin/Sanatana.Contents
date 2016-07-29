using Common.Utility.Pipelines;
using ContentManagementBackend.Resources;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ContentManagementBackend
{
    public class ContentManager<TKey> : IContentManager<TKey>
        where TKey : struct
    {
        //поля
        protected ICommonLogger _logger;


        //свойства
        public InsertContentPipeline<TKey> InsertContentPipeline { get; set; }
        public UpdateContentPipeline<TKey> UpdateContentPipeline { get; set; }
        public DeleteContentPipeline<TKey> DeleteContentPipeline { get; set; }
        
        public ICacheProvider CacheProvider { get; set; }
        public IContentQueries<TKey> ContentQueries { get; set; }
        public ICommentQueries<TKey> CommentQueries { get; set; }
        public ICategoryQueries<TKey> CategoryQueries { get; set; }
        public ISearchQueries<TKey> SearchQueries { get; set; }
        public PreviewImageQueries PreviewImageQueries { get; set; }
        public ContentImageQueries ContentImageQueries { get; set; }
        public CommentImageQueries CommentImageQueries { get; set; }
        public AvatarImageQueries AvatarImageQueries { get; set; }
        
        public ICategoryManager<TKey> CategoryManager { get; set; }
        public ICommentManager<TKey> CommentManager { get; set; }


        //инициализация
        public ContentManager(ICacheProvider cacheProvider, ICommonLogger logger
            , IContentQueries<TKey> contentQueries, ICategoryQueries<TKey> categoryQueries
            , ICommentQueries<TKey> commentQueries, ISearchQueries<TKey> searchQueries
            , PreviewImageQueries previewImageQueries, ContentImageQueries contentImageQueries
            , CommentImageQueries commentImageQueries, AvatarImageQueries avatarImageQueries
            , ICategoryManager<TKey> categoryManager, ICommentManager<TKey> commentManager)
        {
            _logger = logger;

            CacheProvider = cacheProvider;
            ContentQueries = contentQueries;
            CommentQueries = commentQueries;
            CategoryQueries = categoryQueries;
            SearchQueries = searchQueries;

            PreviewImageQueries = previewImageQueries;
            ContentImageQueries = contentImageQueries;
            CommentImageQueries = commentImageQueries;
            AvatarImageQueries = avatarImageQueries;

            CategoryManager = categoryManager;
            CommentManager = commentManager;

            InsertContentPipeline = new InsertContentPipeline<TKey>(contentQueries, previewImageQueries, contentImageQueries, categoryManager, searchQueries);
            UpdateContentPipeline = new UpdateContentPipeline<TKey>(contentQueries, previewImageQueries, contentImageQueries, categoryManager, searchQueries);
            DeleteContentPipeline = new DeleteContentPipeline<TKey>(contentQueries, commentQueries, previewImageQueries, contentImageQueries, categoryManager, searchQueries);
        }



        //Insert 
        public virtual async Task<ContentEditVM<TKey>> Insert(
            ContentSubmitVM contentVM, ContentBase<TKey> content, int permission, List<string> userRoles)
        {
            //query
            Task<ContentPipelineResult> insertQuery = InsertContentPipeline.Process(
                new ContentPipelineModel<TKey>()
                {
                    ContentVM = contentVM,
                    Content = content,
                    UserRoles = userRoles,
                    Permission = permission                    
                });
            Task<QueryResult<List<Category<TKey>>>> categoriesQuery = CategoryManager.SelectIncluded(permission, userRoles);

            ContentPipelineResult insertResult = await insertQuery;
            QueryResult<List<Category<TKey>>> categoriesResult = await categoriesQuery;
            
            //result
            return new ContentEditVM<TKey>()
            {
                ImageUrl = insertResult.ImageUrl,
                UpdateResult = insertResult.Result,
                Error = categoriesResult.HasExceptions
                    ? MessageResources.Common_DatabaseException
                    : insertResult.Message,

                Content = content,
                ContentID = contentVM.ContentID,
                UpdateNonce = contentVM.UpdateNonce,
                MatchUpdateNonce = true,
                ImageStatus = contentVM.ImageStatus,
                ImageID = contentVM.ImageID,

                IsEditPage = false,
                PreviewImageSizeLimit = PreviewImageQueries.Settings.SizeLimit,
                Categories = categoriesResult.Result.ToSelectListItems()
            };
        }



        //Select
        public virtual async Task<ContentFullVM<TKey>> SelectToRead(
            string url, int permission, List<string> userRoles)
        {
            if(string.IsNullOrEmpty(url))
            {
                return new ContentFullVM<TKey>(MessageResources.Content_NotFound);
            }

            //query
            QueryResult<ContentBase<TKey>> queryResult = 
                await ContentQueries.SelectFull(url, true, true);
            
            if (queryResult.HasExceptions)
            {
                return new ContentFullVM<TKey>(MessageResources.Common_DatabaseException);
            }
            if (queryResult.Result == null)
            {
                return new ContentFullVM<TKey>(MessageResources.Content_NotFound);
            }

            //permission
            MessageResult<Category<TKey>> permissionResult = await CategoryManager.CheckPermission(
                queryResult.Result.CategoryID, permission, userRoles);
            if (permissionResult.HasExceptions)
            {
                return new ContentFullVM<TKey>(permissionResult.Message);
            }
            
            return new ContentFullVM<TKey>()
            {
                ContentVM = new ContentRenderVM<TKey>(
                    queryResult.Result, PreviewImageQueries, permissionResult.Result)
            };
        }

        public virtual async Task<ContentFullVM<TKey>> SelectToRead(
            string url, int permission, List<string> userRoles
            , int categoryContentCount, List<ContentFullQuery> queries)
        {
            ContentFullVM<TKey> vm = await SelectToRead(url, permission, userRoles);
            if(vm.HasExceptions)
            {
                return vm;
            }

            List<TKey> categoryIDs = new List<TKey>() { vm.ContentVM.Category.CategoryID };


            //query
            Task<QueryResult<List<ContentBase<TKey>>>> categoryPostsTask = queries.Contains(ContentFullQuery.CategoryLatestPosts)
                ? ContentQueries.SelectShortList(1, categoryContentCount + 1, true, true, categoryIDs)
                : Task.FromResult<QueryResult<List<ContentBase<TKey>>>>(null);

            Task<QueryResult<ContentBase<TKey>>> nextTask = queries.Contains(ContentFullQuery.NextPost)
                ? ContentQueries.SelectShortNext(vm.ContentVM.Content, true, true, categoryIDs)
                : Task.FromResult<QueryResult<ContentBase<TKey>>>(null);

            Task<QueryResult<ContentBase<TKey>>> previousTask = queries.Contains(ContentFullQuery.PreviousPost)
                ? ContentQueries.SelectShortPrevious(vm.ContentVM.Content, true, true, categoryIDs)
                : Task.FromResult<QueryResult<ContentBase<TKey>>>(null);

            QueryResult<List<ContentBase<TKey>>> categoryPosts = await categoryPostsTask;
            QueryResult<ContentBase<TKey>> nextPost = await nextTask;
            QueryResult<ContentBase<TKey>> previousPost = await previousTask;

            //result
            if ((categoryPosts != null && categoryPosts.HasExceptions)
                || (nextPost != null && nextPost.HasExceptions)
                || (previousPost != null && previousPost.HasExceptions))
            {
                vm.Error = MessageResources.Common_DatabaseException;
                vm.HasExceptions = true;
            }
            if (categoryPosts != null)
            {
                url = url.ToLowerInvariant();

                categoryPosts.Result.RemoveAll(p => p.Url == url);
                categoryPosts.Result = categoryPosts.Result.Take(categoryContentCount).ToList();
                                                
                vm.CategoryLatestContent = categoryPosts.Result.Select(
                    p => new ContentRenderVM<TKey>(p, PreviewImageQueries, vm.ContentVM.Category)).ToList();
            }
            if(nextPost != null && nextPost.Result != null)
            {
                vm.NextContentVM = new ContentRenderVM<TKey>(
                    nextPost.Result, PreviewImageQueries, vm.ContentVM.Category);
            }

            if (previousPost != null && previousPost.Result != null)
            {
                vm.PreviousContentVM = new ContentRenderVM<TKey>(
                    previousPost.Result, PreviewImageQueries, vm.ContentVM.Category);
            }
            return vm;
        }

        public virtual async Task<ContentEditVM<TKey>> SelectToEdit(
            TKey contentID, int permission, List<string> userRoles)
        {
            Task<QueryResult<ContentBase<TKey>>> postQuery =
                 ContentQueries.SelectFull(contentID, false, false);
            Task<QueryResult<List<Category<TKey>>>> categoriesQuery =
                CategoryManager.SelectIncluded(permission, userRoles);

            QueryResult<ContentBase<TKey>> postResult = await postQuery;
            QueryResult<List<Category<TKey>>> categoriesResult = await categoriesQuery;

            if (postResult.HasExceptions || categoriesResult.HasExceptions)
            {
                return new ContentEditVM<TKey>()
                {
                    Categories = categoriesResult.Result.ToSelectListItems(),
                    IsEditPage = true,
                    Error = MessageResources.Common_DatabaseException,
                    UpdateResult = ContentUpdateResult.HasException
                };
            }
            if (postResult.Result == null)
            {
                return new ContentEditVM<TKey>()
                {
                    Categories = categoriesResult.Result.ToSelectListItems(),
                    IsEditPage = true,
                    Error = MessageResources.Content_NotFound,
                    UpdateResult = ContentUpdateResult.NotFound
                };
            }
            
            MessageResult<Category<TKey>> permissionResult = await CategoryManager.CheckPermission(
                postResult.Result.CategoryID, permission, userRoles);
            if (permissionResult.HasExceptions)
            {
                return new ContentEditVM<TKey>()
                {
                    Categories = categoriesResult.Result.ToSelectListItems(),
                    IsEditPage = true,
                    Error = permissionResult.Message,
                    UpdateResult = ContentUpdateResult.PermissionException
                };
            }

            var contentVM = ContentEditVM<TKey>.FromContent(postResult.Result);
            contentVM.Categories = categoriesResult.Result.ToSelectListItems();
            contentVM.IsEditPage = true;
            contentVM.ImageUrl = PreviewImageQueries.GetImageUrl(postResult.Result, contentVM, false);
            contentVM.PreviewImageSizeLimit = PreviewImageQueries.Settings.SizeLimit;
            contentVM.OriginalUrl = contentVM.ImageUrl;

            return contentVM;
        }

        public virtual async Task<SelectContentVM<TKey>> SelectPage(
            int page, int pageSize, bool onlyPublished, string categoryUrl
            , int permission, List<string> userRoles, bool countPosts)
        {
            MessageResult<Category<TKey>> permissionResult =
               await CategoryManager.CheckPermission(categoryUrl, permission, userRoles);

            if (permissionResult.HasExceptions)
            {
                return new SelectContentVM<TKey>(permissionResult.Message);
            }

            return await SelectPage(
                page, pageSize, onlyPublished, new List<TKey>() { permissionResult.Result.CategoryID }
                , permission, userRoles, countPosts);
        }

        public virtual async Task<SelectContentVM<TKey>> SelectPage(
            int page, int pageSize, bool onlyPublished, List<TKey> categoryIDs
            , int permission, List<string> userRoles, bool countPosts)
        {
            page = page < 1 ? 1 : page;
            categoryIDs = categoryIDs ?? new List<TKey>();
            
            //filter category permissions
            QueryResult<List<Category<TKey>>> excludeCategories
                = await CategoryManager.SelectExcluded(permission, userRoles);
            List<TKey> excludeCategoryIDs = excludeCategories.Result.Select(p => p.CategoryID).ToList();

            QueryResult<List<TKey>> filteredCategories
                 = await CategoryManager.FilterCategoriesPermission(categoryIDs, permission, userRoles);

            if (excludeCategories.HasExceptions
                || filteredCategories.HasExceptions)
            {
                return new SelectContentVM<TKey>(MessageResources.Common_DatabaseException);
            }

            //query
            Task<QueryResult<List<ContentBase<TKey>>>> articlesTask = ContentQueries.SelectShortList(
                page, pageSize, onlyPublished, true, filteredCategories.Result, excludeCategoryIDs);

            Task<QueryResult<int>> countTask = countPosts
                ? ContentQueries.Count(onlyPublished, filteredCategories.Result, excludeCategoryIDs)
                : Task.FromResult<QueryResult<int>>(null);
            
            Task<QueryResult<List<Category<TKey>>>> categoriesTask = 
                CategoryManager.SelectIncluded(permission, userRoles);

            QueryResult<List<ContentBase<TKey>>> postsResult = await articlesTask;
            QueryResult<int> countResult = await countTask;
            QueryResult<List<Category<TKey>>> categoriesResult = await categoriesTask;
            
            //assemble
            if((postsResult != null && postsResult.HasExceptions)
                || (countResult != null && countResult.HasExceptions)
                || (categoriesResult != null && categoriesResult.HasExceptions))
            {
                return new SelectContentVM<TKey>(MessageResources.Common_DatabaseException);
            }
            
            var vm = new SelectContentVM<TKey>()
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = countResult == null ? 0 : countResult.Result,
                AllCategories = categoriesResult.Result,
                SelectedCategories = categoriesResult.Result.Where(p => categoryIDs.Contains(p.CategoryID)).ToList(),
                ContentRenderVMs = postsResult.Result.Select(p => 
                    new ContentRenderVM<TKey>(p, PreviewImageQueries, categoriesResult.Result))
                    .ToList(),
                PickID = postsResult.Result.Count == 0
                    ? null
                    : postsResult.Result.Last().PublishTimeUtc.ToIso8601(),
                ContentNumberMessage = string.Format(MessageResources.Content_Shown
                    , postsResult.Result.Count, countResult.Result)
            };

            LogMissingCategories(vm.ContentRenderVMs);           
            return vm;
        }

        public virtual async Task<SelectNextContentVM<TKey>> SelectСontinuation(
            string lastID, int pageSize, bool onlyPublished
            , List<TKey> categoryIDs, int permission, List<string> userRoles)
        {
            categoryIDs = categoryIDs ?? new List<TKey>();

            DateTime? lastPublishTimeUtc;            
            bool validDatetime = lastID.TryParseIso8601(out lastPublishTimeUtc);
            if(!validDatetime)
            {
                return new SelectNextContentVM<TKey>(
                    string.Format(MessageResources.Common_DateParseException, nameof(lastID)));
            }

            //filter category permissions
            QueryResult<List<Category<TKey>>> excludeCategories
                 = await CategoryManager.SelectExcluded(permission, userRoles);
            List<TKey> excludeCategoryIDs = excludeCategories.Result.Select(p => p.CategoryID).ToList();

            QueryResult<List<TKey>> filteredCategories
                 = await CategoryManager.FilterCategoriesPermission(categoryIDs, permission, userRoles);

            QueryResult<List<Category<TKey>>> includedCategories
                = await CategoryManager.SelectIncluded(permission, userRoles);

            if (excludeCategories.HasExceptions
                || filteredCategories.HasExceptions
                || includedCategories.HasExceptions)
            {
                return new SelectNextContentVM<TKey>(MessageResources.Common_DatabaseException);
            }

            //query
            int itemsLimit = pageSize + 1;
            QueryResult<List<ContentBase<TKey>>> contents = await
                ContentQueries.SelectShortListСontinuation(lastPublishTimeUtc, itemsLimit, onlyPublished
                , true, filteredCategories.Result, excludeCategoryIDs);

            List<ContentBase<TKey>> items = contents.Result.Take(pageSize).ToList();

            var vm = new SelectNextContentVM<TKey>()
            {
                IsContinued = contents.Result.Count > pageSize,
                ContentRenderVMs = items
                    .Select(p => new ContentRenderVM<TKey>(p, PreviewImageQueries, includedCategories.Result))
                    .ToList(),
                PickID = items.Count == 0
                    ? null
                    : items.Last().PublishTimeUtc.ToIso8601(),
                Error = contents.HasExceptions
                    ? MessageResources.Common_DatabaseException
                    : null
            };
           
            LogMissingCategories(vm.ContentRenderVMs);
            return vm;
        }

        protected virtual void LogMissingCategories(List<ContentRenderVM<TKey>> contents)
        {
            if (contents.Any(p => p.Category == null))
            {
                IEnumerable<TKey> missingCategories = contents
                    .Where(p => p.Category == null).Select(p => p.Content.CategoryID);
                string categoriesList = string.Join(",", missingCategories);
                _logger.Error(InnerMessages.CategoryPostNotMatched, categoriesList);

                contents.RemoveAll(p => p.Category == null);
            }
        }



        //Select Popular
        public virtual async Task<QueryResult<List<ContentRenderVM<TKey>>>> SelectPopular(
            TimeSpan period, int count, int permission, bool useAllPostsToMatchCount)
        {
            List<string> publicRoles = new List<string>();

            //filter category permissions
            QueryResult<List<Category<TKey>>> excludeCategories
                = await CategoryManager.SelectExcluded(permission, publicRoles);
            if (excludeCategories.HasExceptions)
            {
                return new QueryResult<List<ContentRenderVM<TKey>>>(null, true);
            }

            //query popular
            List<TKey> excludeCategoryIDs = excludeCategories.Result.Select(p => p.CategoryID).ToList();

            QueryResult<List<ContentBase<TKey>>> popularPosts
                = await ContentQueries.SelectShortPopular(period, count, excludeCategoryIDs, useAllPostsToMatchCount);
            
            if (popularPosts.HasExceptions)
            {
                return new QueryResult<List<ContentRenderVM<TKey>>>(null, true);
            }            

            //to VM
            QueryResult<List<Category<TKey>>> categories =
               await CategoryManager.SelectIncluded(permission, publicRoles);
            if (categories.HasExceptions)
            {
                return new QueryResult<List<ContentRenderVM<TKey>>>(null, true);
            }

            List<ContentRenderVM<TKey>> postVMs = popularPosts.Result.Select(p => 
                new ContentRenderVM<TKey>(p, PreviewImageQueries, categories.Result))
                .OrderByDescending(p => p.Content.ViewsCount)
                .ToList();            
            return new QueryResult<List<ContentRenderVM<TKey>>>(postVMs, false);
        }
        


        //Update
        public virtual async Task<ContentEditVM<TKey>> Update(
            ContentSubmitVM contentVM, ContentBase<TKey> content, int permission, List<string> userRoles)
        {
            //query
            Task<ContentPipelineResult> updateQuery = UpdateContentPipeline.Process(
                new ContentPipelineModel<TKey>()
                {
                    ContentVM = contentVM,
                    Content = content,
                    UserRoles = userRoles,
                    Permission = permission
                });
            Task<QueryResult<List<Category<TKey>>>> categoriesQuery = CategoryManager.SelectIncluded(permission, userRoles);

            ContentPipelineResult updateResult = await updateQuery;
            QueryResult<List<Category<TKey>>> categoriesResult = await categoriesQuery;

            //result
            return new ContentEditVM<TKey>()
            {
                ImageUrl = updateResult.ImageUrl,
                UpdateResult = updateResult.Result,
                Error = categoriesResult.HasExceptions
                    ? MessageResources.Common_DatabaseException
                    : updateResult.Message,

                Content = content,
                ContentID = contentVM.ContentID,
                UpdateNonce = contentVM.UpdateNonce,
                MatchUpdateNonce = true,
                ImageStatus = contentVM.ImageStatus,
                ImageID = contentVM.ImageID,

                IsEditPage = true,
                PreviewImageSizeLimit = PreviewImageQueries.Settings.SizeLimit,
                Categories = categoriesResult.Result.ToSelectListItems()
            };
        }



        //Delete
        public virtual Task<PipelineResult> Delete(TKey contentID, int permission, List<string> userRoles)
        {
            return DeleteContentPipeline.Process(new ContentDeletePipelineModel<TKey>()
            {
                ContentID = contentID,
                UserRoles = userRoles,
                Permission = permission                
            });
        }



        //Search
        public virtual async Task<SearchResultVM<TKey>> Search(
            SearchInputVM<TKey> vm, int pageSize, int permission, List<string> userRoles)
        {   
            vm.Page = vm.Page < 1 ? 1 : vm.Page;

            //check category permissions
            if (vm.CategoryID != null)
            {
                MessageResult<Category<TKey>> permissionResult =
                    await CategoryManager.CheckPermission(vm.CategoryID.Value, permission, userRoles);

                if (permissionResult.HasExceptions)
                {
                    return new SearchResultVM<TKey>(permissionResult.Message);
                }
            }

            //search + categories
            SearchResponse<TKey> searchResult = string.IsNullOrEmpty(vm.Input)
                ? new SearchResponse<TKey>() { Content = new List<ContentBase<TKey>>() }
                : await SearchQueries.Find(vm.Input, vm.Page, pageSize, true, vm.CategoryID);

            QueryResult<List<Category<TKey>>> categoriesResult = await
                CategoryManager.SelectIncluded(permission, userRoles);
            
            if (searchResult.HasExceptions || categoriesResult.HasExceptions)
            {
                return new SearchResultVM<TKey>(MessageResources.Common_DatabaseException);
            }

            if (searchResult.Content.Count == 0)
            {
                return new SearchResultVM<TKey>()
                {
                    Input = vm.Input,
                    CategoryID = vm.CategoryID,
                    Categories = categoriesResult.Result.ToSelectListItems(),
                    Content = new List<ContentRenderVM<TKey>>()
                };
            }
            

            //contents
            List<TKey> contentIDs = searchResult.Content.Select(p => p.ContentID).ToList();
            QueryResult<List<ContentBase<TKey>>> postsResult =
                await ContentQueries.SelectShortList(contentIDs, true, true);

            if (postsResult.HasExceptions)
            {
                return new SearchResultVM<TKey>(MessageResources.Common_DatabaseException);
            }
            

            //merge results
            List<ContentBase<TKey>> contentOrdered = MergeSearchResults(searchResult, postsResult);

            return new SearchResultVM<TKey>()
            {
                Input = vm.Input,
                CategoryID = vm.CategoryID,
                Page = vm.Page,
                PageSize = pageSize,
                Categories = categoriesResult.Result.ToSelectListItems(),
                TotalItems = (int)searchResult.Total,
                Content = contentOrdered.Select(p => 
                    new ContentRenderVM<TKey>(p, PreviewImageQueries, categoriesResult.Result)).ToList()
            };
        }

        protected virtual List<ContentBase<TKey>> MergeSearchResults(
            SearchResponse<TKey> searchResult, QueryResult<List<ContentBase<TKey>>> postsResult)
        {
            ContentBase<TKey> content = searchResult.Content[0];
            string titleFKey = nameof(content.Title);
            string fullContentFKey = nameof(content.FullContent);

            for (int a = 0; a < postsResult.Result.Count; a++)
            {
                content = postsResult.Result[a];
                int searchIndex = searchResult.Content.FindIndex(p =>
                    EqualityComparer<TKey>.Default.Equals(p.ContentID, content.ContentID));

                //content present in search index, but not in storage, skip it
                if (searchIndex == -1)
                {
                    continue;
                }

                if (searchResult.HighlightedFieldNames[searchIndex].Contains(titleFKey))
                {
                    content.Title = searchResult.Content[searchIndex].Title;
                }

                if (searchResult.HighlightedFieldNames[searchIndex].Contains(fullContentFKey))
                {
                    content.ShortContent = searchResult.Content[searchIndex].FullContent;
                }
            }

            List<ContentBase<TKey>> contentOrdered = (from i in searchResult.Content
                                                    join o in postsResult.Result
                                                    on i.ContentID equals o.ContentID
                                                    select o).ToList();
            return contentOrdered;
        }



    }
}
