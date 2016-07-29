using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContentManagementBackend
{
    public class CommentRenderVM<TKey>
        where TKey : struct
    {
        //свойства
        public TKey CommentID { get; set; }
        public string AddTimeUtc { get; set; }

        public TKey? BranchCommentID { get; set; }
        public TKey? ParentCommentID { get; set; }
        public string Content { get; set; }
        public int UpVotes { get; set; }
        public int DownVotes { get; set; }

        public TKey? AuthorID { get; set; }
        public string AuthorName { get; set; }
        public string AuthorAvatar { get; set; }
        
        public List<CommentRenderVM<TKey>> Children { get; set; }

        public string SubjectTitle { get; set; }
        public string Url { get; set; }


        //инициализация
        public CommentRenderVM()
        {
        }

        public CommentRenderVM(Comment<TKey> comment
            , AvatarImageQueries avatarQueries)
        {
            CommentID = comment.CommentID;
            AddTimeUtc = comment.AddTimeUtc.ToIso8601();

            ParentCommentID = comment.ParentCommentID;
            BranchCommentID = comment.BranchCommentID;
            Content = comment.Content;
            UpVotes = comment.UpVotes;
            DownVotes = comment.DownVotes;

            AuthorID = comment.AuthorID;
            AuthorName = comment.AuthorName;
            AuthorAvatar = avatarQueries.PathCreator.CreateStaticUrl(comment.AuthorAvatar);
        }

        public CommentRenderVM(Comment<TKey> comment
            , AvatarImageQueries avatarQueries, ContentBase<TKey> subject)
            : this(comment, avatarQueries)
        {
            if (subject != null)
            {
                SubjectTitle = subject.Title;
                Url = subject.Url;
            }
        }

        public CommentRenderVM(Comment<TKey> comment
            , AvatarImageQueries avatarQueries, List<IGrouping<TKey?, Comment<TKey>>> childrenGroups = null)
            : this(comment, avatarQueries)
        {
            IGrouping<TKey?, Comment<TKey>> replies = childrenGroups == null 
                ? null 
                : childrenGroups.FirstOrDefault(p => p.Key.HasValue &&
                    EqualityComparer<TKey>.Default.Equals(p.Key.Value, CommentID));
            
            Children = replies == null
                ? null
                : replies.Select(p => new CommentRenderVM<TKey>(p, avatarQueries)).ToList();
        }


    }
}