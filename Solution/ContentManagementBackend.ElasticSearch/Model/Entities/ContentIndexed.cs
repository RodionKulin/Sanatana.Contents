using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.ElasticSearch
{
    public class ContentIndexed<TKey>
        where TKey : struct
    {
        //свойства
        public TKey CategoryID { get; set; }
        public string Title { get; set; }
        public string FullContent { get; set; }



        //инициализация
        public ContentIndexed()
        {
        }
        public ContentIndexed(ContentBase<TKey> item)
        {
            CategoryID = item.CategoryID;
            Title = item.Title;
            FullContent = HtmlTagRemover.StripHtml(item.FullContent);
        }
    }
}
