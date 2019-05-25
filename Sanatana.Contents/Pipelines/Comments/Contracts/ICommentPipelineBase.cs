using System.Threading.Tasks;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Patterns.Pipelines;

namespace Sanatana.Contents.Pipelines.Comments
{
    public interface ICommentPipelineBase<TKey, TCategory, TContent, TComment>
        : IPipeline<CommentEditParams<TKey, TComment>, PipelineResult>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
        where TComment : Comment<TKey>
    {
        Task<bool> CheckPermission(PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context);
        Task<bool> GetStoredComment(PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context);
        Task<bool> IncrementVersion(PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context);
        Task<bool> Sanitize(PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context);
        Task<bool> UpdateCommentSearch(PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context);
        Task<bool> ValidateComment(PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context);
    }
}