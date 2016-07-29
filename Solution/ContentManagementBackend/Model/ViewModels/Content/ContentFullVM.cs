using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class ContentFullVM<TKey>
        where TKey : struct
    {
        //свойства
        public string Error { get; set; }
        public bool HasExceptions { get; set; }
        public ContentRenderVM<TKey> ContentVM { get; set; }
        public ContentRenderVM<TKey> NextContentVM { get; set; }
        public ContentRenderVM<TKey> PreviousContentVM { get; set; }
        public List<ContentRenderVM<TKey>> CategoryLatestContent { get; set; }



        //инициализация
        public ContentFullVM()
        {
        }
        public ContentFullVM(string error)
        {
            Error = error;
            HasExceptions = true;
        }
    }
}
