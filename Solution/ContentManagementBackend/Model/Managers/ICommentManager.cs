using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Utility.Pipelines;
using Common.Utility;

namespace ContentManagementBackend
{
    public interface ICommentManager<TKey> where TKey : struct
    {
        ICaptchaProvider CaptchaProvider { get; set; }
        ICommentStateProvider CommentStateProvider { get; set; }
        DeleteCommentPipeline<TKey> DeleteCommentPipeline { get; set; }
        InsertCommentPipeline<TKey> InsertCommentPipeline { get; set; }
        UpdateCommentPipeline<TKey> UpdateCommentPipeline { get; set; }

        Task<PipelineResult> Delete(CommentPipelineModel<TKey> vm);
        Task<PipelineResult> Insert(CommentPipelineModel<TKey> vm);
        Task<PipelineResult> Update(CommentPipelineModel<TKey> vm);
        Task<QueryResult<List<CommentRenderVM<TKey>>>> SelectLatest(int count, bool publicVisible);
        Task<MessageResult<List<CommentRenderVM<TKey>>>> SelectForSubject(
            TKey contentID, TKey categoryID, int permission, List<string> userRoles);
    }
}