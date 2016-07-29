using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ContentManagementBackend
{
    public class ContentRenderVM<TKey>
        where TKey : struct
    {
        //свойства
        public ContentBase<TKey> Content { get; set; }
        public Category<TKey> Category { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        

        //зависимые свойства
        public string PublishTimeIso8601
        {
            get
            {
                return Content.PublishTimeUtc.ToIso8601();
            }
        }
        
        
        
        private ContentRenderVM()
        {

        }
        public ContentRenderVM(ContentBase<TKey> content
            , PreviewImageQueries imageQueries, List<Category<TKey>> categories)
        {
            Content = content;
            Category = categories.FirstOrDefault(p =>
                EqualityComparer<TKey>.Default.Equals(p.CategoryID, content.CategoryID));
            ImageUrl = content.HasImage
                ? imageQueries.PathCreator.CreateStaticUrl(content.Url)
                : null;
        }
        public ContentRenderVM(ContentBase<TKey> content
            , PreviewImageQueries imageQueries, Category<TKey> category)
        {
            Content = content;
            Category = category;
            ImageUrl = content.HasImage
                ? imageQueries.PathCreator.CreateStaticUrl(content.Url)
                : null;
        }
    }
}
