using System.Threading.Tasks;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Patterns.Pipelines;

namespace Sanatana.Contents.Pipelines.Comments
{
    public interface IDeleteCommentPipeline<TKey, TCategory, TContent, TComment>
        : ICommentPipelineBase<TKey, TCategory, TContent, TComment>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
        where TComment : Comment<TKey>
    {
        Task<bool> DeleteCommentDb(PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context);
        Task<bool> DeleteCommentSearch(PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context);
        Task<bool> RollBackDeleteCommentDb(PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context);
        Task<bool> RollBackDeleteCommentSearch(PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context);
        Task<bool> RollBackUpdateTotalCountDb(PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context);
        Task<bool> UpdateTotalCountDb(PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context);
    }
}