using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ContentManagementBackend
{
    public class SearchResultVM<TKey> : SearchInputVM<TKey>, IPagerVM
        where TKey : struct
    {
        //свойства
        public string Error { get; set; }
        public List<SelectListItem> Categories { get; set; }
        public List<ContentRenderVM<TKey>> Content { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }


        //зависимые свойства
        public int TotalPages
        {
            get
            {
                return (int)Math.Ceiling((double)TotalItems / PageSize);
            }
        }



        //инициализация
        public SearchResultVM()
        {
        }
        public SearchResultVM(string error)
        {
            Error = error;
        }
        public SearchResultVM(SearchInputVM<TKey> vm)
        {
            Input = vm.Input;
            CategoryID = vm.CategoryID;
            Page = vm.Page;
        }
    }
}