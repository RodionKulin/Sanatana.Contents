using ContentManagementBackend.Demo.App_Resources;
using ContentManagementBackend.Resources;
using Common.Utility;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ContentManagementBackend.Demo.Controllers
{
    public class SearchController : BaseController
    {
        //инициализация
        public SearchController(CustomContentManager contentManager)
            : base(contentManager)
        {
            _postManager = contentManager;
        }



        //методы
        public async Task<ActionResult> Index(SearchInputVM<ObjectId> inputVM)
        {
            inputVM.CategoryID = inputVM.CategoryID == ObjectId.Empty ? null : inputVM.CategoryID;
            List<string> userRoles = new List<string>(); //only public categories are indexed, so no roles required           

            Task<bool> sidebarTask = SelectSidebarVMData();
            Task<SearchResultVM<ObjectId>> searchTask = _postManager.Search(inputVM
                , SiteConstants.SEARCH_LIST_PAGE_SIZE, (int)CategoryPermission.View, userRoles);

            bool isSidebarSet = await sidebarTask;
            SearchResultVM<ObjectId> searchResult = await searchTask;

            if (!string.IsNullOrEmpty(searchResult.Error))
            {
                return View("_Message", new MessageContentVM()
                {
                    Title = GlobalContent.Search_Index_Title,
                    Message = searchResult.Error,
                    IsError = true
                });
            }

            searchResult.Categories.Insert(0, new System.Web.Mvc.SelectListItem()
            {
                Text = GlobalContent.Search_Index_AllCategories,
                Value = ObjectId.Empty.ToString()
            });

            foreach (ContentRenderVM<ObjectId> item in searchResult.Content)
            {
                item.Content.Url = SetFullUrl(SiteConstants.URL_BASE_FULL_POST, item.Content.Url);
            }

            return View(searchResult);
        }
    }
}