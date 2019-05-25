using System.Threading.Tasks;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Patterns.Pipelines;

namespace Sanatana.Contents.Pipelines.Comments
{
    public interface IUpdateCommentPipeline<TKey, TCategory, TContent, TComment>
        : ICommentPipelineBase<TKey, TCategory, TContent, TComment>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
        where TComment : Comment<TKey>
    {
        Task<bool> RollBackUpdateCommentDb(PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context);
        Task<bool> RollBackUpdateCommentSearch(PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context);
        Task<bool> UpdateCommentDb(PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context);
    }
}