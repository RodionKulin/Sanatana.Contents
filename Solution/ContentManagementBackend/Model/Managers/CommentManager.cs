using Common.Utility.Pipelines;
using ContentManagementBackend.Resources;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class CommentManager<TKey> : ICommentManager<TKey>
        where TKey : struct
    {
        //свойства
        public InsertCommentPipeline<TKey> InsertCommentPipeline { get; set; }
        public UpdateCommentPipeline<TKey> UpdateCommentPipeline { get; set; }
        public DeleteCommentPipeline<TKey> DeleteCommentPipeline { get; set; }

        public ICaptchaProvider CaptchaProvider { get; set; }
        public ICommentStateProvider CommentStateProvider { get; set; }

        public IContentQueries<TKey> PostQueries { get; set; }
        public ICommentQueries<TKey> CommentQueries { get; set; }
        public AvatarImageQueries AvatarImageQueries { get; set; }
        public ICategoryManager<TKey> CategoryManager { get; set; }


        //инициализация
        public CommentManager(IContentQueries<TKey> contentQueries, ICommentQueries<TKey> commentQueries
            , ICategoryManager<TKey> categoryManager, AvatarImageQueries avatarImageQueries
            , ICommentStateProvider commentStateProvider, ICaptchaProvider captchaProvider)
        {
            PostQueries = contentQueries;
            CommentQueries = commentQueries;
            AvatarImageQueries = avatarImageQueries;
            CategoryManager = categoryManager;

            CaptchaProvider = captchaProvider;
            CommentStateProvider = commentStateProvider;

            InsertCommentPipeline = new InsertCommentPipeline<TKey>(contentQueries, commentQueries, categoryManager, captchaProvider, commentStateProvider);
            UpdateCommentPipeline = new UpdateCommentPipeline<TKey>(contentQueries, commentQueries, categoryManager, captchaProvider, commentStateProvider);
            DeleteCommentPipeline = new DeleteCommentPipeline<TKey>(contentQueries, commentQueries, categoryManager, captchaProvider, commentStateProvider);

        }


        //Comments
        public virtual Task<PipelineResult> Insert(CommentPipelineModel<TKey> vm)
        {
            return InsertCommentPipeline.Process(vm);
        }

        public virtual Task<PipelineResult> Update(CommentPipelineModel<TKey> vm)
        {
            return UpdateCommentPipeline.Process(vm);
        }

        public virtual Task<PipelineResult> Delete(CommentPipelineModel<TKey> vm)
        {
            return DeleteCommentPipeline.Process(vm);
        }

        public virtual async Task<QueryResult<List<CommentRenderVM<TKey>>>> SelectLatest(
            int count, bool publicVisible)
        {
            List<CommentState> states = CommentStateProvider.List(true);
            bool onlyPublished = true;

            QueryResult<List<Comment<TKey>>> comments =
                await CommentQueries.Select(states, 1, count);
            if (comments.HasExceptions)
            {
                return new QueryResult<List<CommentRenderVM<TKey>>>(null, true);
            }

            List<TKey> contentIDs = comments.Result.Select(p => p.ContentID).ToList();
            QueryResult<List<ContentBase<TKey>>> titlesResult =
                await PostQueries.SelectShortList(contentIDs, onlyPublished, false);

            List<CommentRenderVM<TKey>> vmList = new List<CommentRenderVM<TKey>>();
            foreach (Comment<TKey> comment in comments.Result)
            {
                ContentBase<TKey> subject = titlesResult.Result.FirstOrDefault(p =>
                    EqualityComparer<TKey>.Default.Equals(p.ContentID, comment.ContentID));

                if (subject != null)
                {
                    vmList.Add(new CommentRenderVM<TKey>(comment, AvatarImageQueries, subject));
                }
            }

            return new QueryResult<List<CommentRenderVM<TKey>>>(vmList, titlesResult.HasExceptions);
        }

        public virtual async Task<MessageResult<List<CommentRenderVM<TKey>>>> SelectForSubject(
            TKey contentID, TKey categoryID, int permission, List<string> userRoles)
        {
            MessageResult<Category<TKey>> categoryPermission = await CategoryManager.CheckPermission(categoryID, permission, userRoles);
            if (categoryPermission.HasExceptions)
            {
                return new MessageResult<List<CommentRenderVM<TKey>>>(null, categoryPermission.Message, true);
            }

            bool isPublic = CategoryManager.CheckIsPublic(categoryPermission.Result, permission);
            List<CommentState> states = CommentStateProvider.List(isPublic);

            QueryResult<List<Comment<TKey>>> comments = await CommentQueries.Select(states, 1, int.MaxValue, contentID);
            if (comments.HasExceptions)
            {
                return new MessageResult<List<CommentRenderVM<TKey>>>(null, MessageResources.Common_DatabaseException, true);
            }

            List<IGrouping<TKey?, Comment<TKey>>> commentGroups =
                comments.Result.GroupBy(p => p.BranchCommentID).ToList();

            IGrouping<TKey?, Comment<TKey>> rootGroup =
                commentGroups.FirstOrDefault(p => p.Key == null);

            List<CommentRenderVM<TKey>> list = rootGroup == null
                ? null
                : rootGroup.Select(p => new CommentRenderVM<TKey>(p, AvatarImageQueries, commentGroups))
                    .OrderByDescending(p => p.AddTimeUtc)
                    .ToList();

            return new MessageResult<List<CommentRenderVM<TKey>>>(list, null, false);
        }

    }
}
