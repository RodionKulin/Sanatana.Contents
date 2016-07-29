using Common.Utility;
using ContentManagementBackend.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class ContentBase<TKey>
        where TKey : struct
    {
        //свойства
        public virtual TKey ContentID { get; set; }
        [Display(ResourceType = typeof(MessageResources), Name = "Content_CategoryID")]
        public virtual TKey CategoryID { get; set; }
        public virtual string UpdateNonce { get; set; }
        [Display(ResourceType = typeof(MessageResources), Name = "Content_Title")]
        public virtual string Title { get; set; }
        [Display(ResourceType = typeof(MessageResources), Name = "Content_FullText")]
        public virtual string FullContent { get; set; }
        [Display(ResourceType = typeof(MessageResources), Name = "Content_ShortText")]
        public virtual string ShortContent { get; set; }
        public virtual bool HasImage { get; set; }
        public virtual string Url { get; set; }
        public virtual TKey AuthorID { get; set; }
        [Display(ResourceType = typeof(MessageResources), Name = "Content_AuthorName")]
        public virtual string AuthorName { get; set; }
        public virtual DateTime AddTimeUtc { get; set; }
        [Display(ResourceType = typeof(MessageResources), Name = "Content_PublishTimeUtc")]
        public virtual DateTime PublishTimeUtc { get; set; }
        [Display(ResourceType = typeof(MessageResources), Name = "Content_IsPublished")]
        public virtual bool IsPublished { get; set; }
        public virtual bool IsIndexed { get; set; }
        [Display(ResourceType = typeof(MessageResources), Name = "Content_CommentsCount")]
        public virtual int CommentsCount { get; set; }
        [Display(ResourceType = typeof(MessageResources), Name = "Content_ViewsCount")]
        public virtual int ViewsCount { get; set; }




        //методы
        public void CreateUpdateNonce()
        {
            UpdateNonce = ShortGuid.NewGuid();
        }
    }
}
