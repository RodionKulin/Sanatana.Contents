using ContentManagementBackend.Resources;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class SelectContentVM<TKey> : IPagerVM
        where TKey : struct
    {

        //свойства
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public string PickID { get; set; }
        public string ContentNumberMessage { get; set; }
        public string Error { get; set; }
        public List<ContentRenderVM<TKey>> ContentRenderVMs { get; set; }
        public List<CommentRenderVM<TKey>> Comments { get; set; }
        public List<Category<TKey>> AllCategories { get; set; }
        public List<Category<TKey>> SelectedCategories { get; set; }



        //вычисляемые свойства
        public int TotalPages
        {
            get
            {
                return (int)Math.Ceiling((double)TotalItems / PageSize);
            }
        }
        public bool IsPageOutOfRange
        {
            get
            {
                bool isZeroItems = TotalPages == 0 && Page == 1;
                return Page > TotalPages && !isZeroItems;
            }
        }
        public bool IsLastPage
        {
            get
            {
                return Page >= TotalPages;
            }
        }
        public string SelectedCategoryIDsJSArray
        {
            get
            {
                if (SelectedCategories == null)
                    return null;

                List<string> idStrings = SelectedCategories
                    .Select(p => string.Format("'{0}'", p.CategoryID.ToString()))
                    .ToList();

                return string.Format("[{0}]", string.Join(",", idStrings));
            }
        }


        //инициализация
        public SelectContentVM()
        {
            ContentRenderVMs = new List<ContentRenderVM<TKey>>();
            Comments = new List<CommentRenderVM<TKey>>();
            AllCategories = new List<Category<TKey>>();
            SelectedCategories = new List<Category<TKey>>();
        }
        public SelectContentVM(string error)
            : this()
        {
            Error = error;
        }
    }
}
