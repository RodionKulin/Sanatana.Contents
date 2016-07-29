using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class CommentPipelineModel<TKey>
        where TKey : struct
    {
        //свойства
        public Comment<TKey> Comment { get; set; }
        public string Challenge { get; set; }
        public string Response { get; set; }
        public List<string> AllowedIFrameUrls { get; set; }
        public List<string> UserRoles { get; set; }
        public int Permission { get; set; }
        public int ViewPermission { get; set; }



        //инициализация
        public CommentPipelineModel()
        {
            AllowedIFrameUrls = new List<string>()
            {
                @"youtube.\w*"
            };
        }
    }
}
