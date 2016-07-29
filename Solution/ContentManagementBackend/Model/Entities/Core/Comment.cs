using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContentManagementBackend;

namespace ContentManagementBackend
{
    public class Comment<TKey>
        where TKey : struct
    {
        //свойства
        public TKey CommentID { get; set; }
        public TKey ContentID { get; set; }
        public CommentState State { get; set; }
        public DateTime AddTimeUtc { get; set; }
        public DateTime UserUpdateTimeUtc { get; set; }

        public TKey? BranchCommentID { get; set; }
        public TKey? ParentCommentID { get; set; }
        public string Content { get; set; }
        public int UpVotes { get; set; }
        public int DownVotes { get; set; }

        public TKey? AuthorID { get; set; }
        public string AuthorName { get; set; }
        public string AuthorAvatar { get; set; }


        //инициализация
        public Comment()
        {
        }
        public Comment(CommentVM<TKey> vm)
        {
            ContentID = vm.ContentID;
            Content = vm.Content;
            ParentCommentID = vm.ParentCommentID;
            BranchCommentID = vm.BranchCommentID;
            AddTimeUtc = DateTime.UtcNow;
            UserUpdateTimeUtc = AddTimeUtc;
        }

                
    }
}
