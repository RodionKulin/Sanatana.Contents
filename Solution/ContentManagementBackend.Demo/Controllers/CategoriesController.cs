using ContentManagementBackend.Demo.App_Resources;
using ContentManagementBackend.Resources;
using Common.Utility;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ContentManagementBackend.Demo.Controllers
{
    public class CategoriesController : BaseController
    {
        //инициализация
        public CategoriesController(CustomContentManager contentManager)
            : base(contentManager)
        {
        }


        //методы        
        public async Task<ActionResult> List()
        {
            _postManager.CacheProvider.ClearAll();
            QueryResult<List<Category<ObjectId>>> categories = await _postManager.CategoryManager.SelectSorted();

            if (categories.HasExceptions)
            {
                return View("_Message", new MessageContentVM()
                {
                    Title = GlobalContent.Categories_List_Title,
                    Message = MessageResources.Common_DatabaseException,
                    IsError = true
                });
            }

            return View(categories.Result);
        }
        
    }
}