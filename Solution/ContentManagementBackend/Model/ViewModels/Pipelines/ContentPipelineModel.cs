using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class ContentPipelineModel<TKey>
        where TKey : struct
    {
        //свойства
        public ContentSubmitVM ContentVM { get; set; }
        public ContentBase<TKey> Content { get; set; }
        public int MaxTitleLength { get; set; } = Constants.MAX_TITLE_LENGTH;
        public int MaxShortContentLength { get; set; } = Constants.MAX_SHORT_CONTENT_LENGTH;
        public List<string> AllowedIFrameUrls { get; set; }
        public List<string> UserRoles { get; set; }
        public int Permission { get; set; }



        //инициализация
        public ContentPipelineModel()
        {
            AllowedIFrameUrls = new List<string>()
            {
                @"youtube.\w*"
            };
        }
        public ContentPipelineModel(ContentEditVM<TKey> contentVM
            , int? maxTitleLength =  null, int? maxShortContentLength = null)
            : this()
        {
            ContentVM = contentVM;
            MaxTitleLength = maxTitleLength ?? Constants.MAX_TITLE_LENGTH;
            MaxShortContentLength = maxShortContentLength ?? Constants.MAX_SHORT_CONTENT_LENGTH;
        }
    }
}
