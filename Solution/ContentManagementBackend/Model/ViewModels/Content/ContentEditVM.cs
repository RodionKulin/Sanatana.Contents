using ContentManagementBackend.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ContentManagementBackend
{
    public class ContentEditVM<TKey> : ContentSubmitVM
        where TKey : struct
    {
        //свойства
        public string Error { get; set; }
        public List<SelectListItem> Categories { get; set; }
        public bool IsEditPage { get; set; }
        public ContentUpdateResult UpdateResult  { get; set; }
        public ContentBase<TKey> Content { get; set; }
        public long PreviewImageSizeLimit { get; set; }
        public string ImageUrl { get; set; }
        public string OriginalUrl { get; set; }


        //зависимые свойства
        public string PublishTimeIso8601
        {
            get
            {
                return Content.PublishTimeUtc == null
                    ? null
                    : Content.PublishTimeUtc.ToIso8601();
            }
        }



        //инициализация
        public ContentEditVM()
        {
        }

        public static ContentEditVM<TKey> FromContent(ContentBase<TKey> content)
        {
            return new ContentEditVM<TKey>()
            {
                Content = content,
                ContentID = content.ContentID.ToString(),              
                ImageStatus = content.HasImage ? ImageStatus.Static : ImageStatus.NotSet,               
                MatchUpdateNonce = true
            };
        }

    }
}
