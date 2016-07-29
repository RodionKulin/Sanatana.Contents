using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Imaging;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using MongoDB.Bson;
using System.Web.UI;
using Common.Identity2_1;
using Common.Web;
using Common.Utility;
using ContentManagementBackend;
using ContentManagementBackend.Resources;
using ContentManagementBackend.Demo.App_Resources;
using Common.Utility.Pipelines;

namespace ContentManagementBackend.Demo.Controllers
{
    public class PostsController : BaseController
    {
        //поля
        private IContentEncryption _contentEncryption;


        //инициализация
        public PostsController(CustomContentManager contentManager
            , IContentEncryption contentEncryption)
            : base(contentManager)
        {
            _contentEncryption = contentEncryption;
        }



        //posts
        public async Task<ActionResult> Full(string id)
        {
            List<string> userRoles = _userRoles.Select(p => p.ToString()).ToList();

            Task<bool> sidebarTask = SelectSidebarVMData();
            Task<ContentFullVM <ObjectId>> contentTask = _postManager.SelectToRead(
                id, (int)CategoryPermission.View, userRoles
                , SiteConstants.POSTS_FULL_CATEGORY_LIST_COUNT, new List<ContentFullQuery>()
                {
                    ContentFullQuery.CategoryLatestPosts
                });

            bool isSidebarSet = await sidebarTask;
            ContentFullVM<ObjectId> post = await contentTask;

            if (post.HasExceptions || !isSidebarSet)
            {
                return View("_Message", new MessageContentVM()
                {
                    Title = GlobalContent.Post_Index_PostNotFoundTitle,
                    Message = new [] { post.Error, MessageResources.Common_DatabaseException }.First(p => p != null),
                    IsError = true
                });
            }

            foreach (ContentRenderVM<ObjectId> item in post.CategoryLatestContent)
            {
                item.Url = SetFullUrl(SiteConstants.URL_BASE_FULL_POST, item.Content.Url);
            }

            return View(post);
        }

        public async Task<ActionResult> List(int page = 1)
        {
            List<string> userRoles = new List<string>();    //only public posts on home page

            Task<bool> sidebarTask = SelectSidebarVMData();
            Task<SelectContentVM<ObjectId>> postsTask = _postManager.SelectPage(
                page, SiteConstants.POSTS_LIST_PAGE_SIZE, true
                , new List<ObjectId>(), (int)CategoryPermission.View, userRoles, true);

            bool isSidebarSet = await sidebarTask;
            SelectContentVM<ObjectId> posts = await postsTask;

            if (!string.IsNullOrEmpty(posts.Error) || !isSidebarSet)
            {
                return View("_Message", new MessageContentVM()
                {
                    Title = GlobalContent.Post_List_Title,
                    Message = new[] { posts.Error, MessageResources.Common_DatabaseException }.First(p => p != null),
                    IsError = true
                });
            }

            foreach (ContentRenderVM<ObjectId> item in posts.ContentRenderVMs)
            {
                item.Url = SetFullUrl(SiteConstants.URL_BASE_FULL_POST, item.Content.Url);
            }
            
            return View(posts);
        }

        public async Task<ActionResult> Category(string categoryUrl, int page = 1)
        {
            List<string> userRoles = _userRoles.Select(p => p.ToString()).ToList();

            Task<bool> sidebarTask = SelectSidebarVMData();
            Task<SelectContentVM<ObjectId>> postsTask = _postManager.SelectPage(
                page, SiteConstants.POSTS_LIST_PAGE_SIZE, true
                , categoryUrl, (int)CategoryPermission.View, userRoles, true);

            bool isSidebarSet = await sidebarTask;
            SelectContentVM<ObjectId> posts = await postsTask;

            if (!string.IsNullOrEmpty(posts.Error))
            {
                return View("_Message", new MessageContentVM()
                {
                    Title = posts.SelectedCategories.Count > 0
                        ? posts.SelectedCategories.First().Name
                        : GlobalContent.Post_List_Title,
                    Message = posts.Error,
                    IsError = true
                });
            }

            foreach (ContentRenderVM<ObjectId> item in posts.ContentRenderVMs)
            {
                item.Url = SetFullUrl(SiteConstants.URL_BASE_FULL_POST, item.Content.Url);
            }

            bool isPublic = _postManager.CategoryManager.CheckIsPublic(
                posts.SelectedCategories.First(), (int)CategoryPermission.View);

            return isPublic 
                ? View(posts) 
                : View("EditList", posts);
        }

        [HttpPost]
        public async Task<ActionResult> AjaxList(string lastID, List<ObjectId> categoryIDs)
        {
            categoryIDs = categoryIDs ?? new List<ObjectId>();

            List<string> userRoles = categoryIDs.Count == 0
                ? new List<string>()    //only public posts on home page
                : _userRoles.Select(p => p.ToString()).ToList();
            
            SelectNextContentVM<ObjectId> postsVM = await _postManager.SelectСontinuation(
                lastID, SiteConstants.POSTS_AJAX_LIST_PAGE_SIZE, true
                , categoryIDs, (int)CategoryPermission.View, userRoles);

            return new JsonCamelCaseResult()
            {
                Data = new {
                    isContinued = postsVM.IsContinued,
                    pickID = postsVM.PickID,
                    error = postsVM.Error,
                    posts = postsVM.ContentRenderVMs.Select(p => new {
                        id = p.Content.ContentID,
                        url = SiteConstants.URL_BASE_FULL_POST + p.Content.Url,
                        imageUrl = p.ImageUrl,
                        showImage = !(p.Content is YoutubePost<ObjectId>),
                        title = p.Content.Title,
                        shortContent = p.Content.ShortContent,
                        publishTimeUtc = p.Content.PublishTimeUtc,
                        commentsCount = p.Content.CommentsCount,
                        viewsCount = p.Content.ViewsCount,
                        categoryUrl = Url.Action("Category", "Posts", new { categoryUrl = p.Category.Url }),
                        categoryName = p.Category.Name
                    })
                },
                NullValueHandling = NullValueHandling.Ignore
            };

           
        }
        
        public async Task<ActionResult> EditList(int page = 1, ObjectId? categoryID = null)
        {
            List<string> userRoles = _userRoles.Select(p => p.ToString()).ToList();
            
            SelectContentVM<ObjectId> vm = await _postManager.SelectPage(
                page, SiteConstants.POSTS_MODERATION_PAGE_SIZE, false
                , new List<ObjectId>(), (int)CategoryPermission.Edit, userRoles, true);
            
            if (!string.IsNullOrEmpty(vm.Error))
            {
                return View("_Message", new MessageContentVM()
                {
                    Title = GlobalContent.Post_Moderation_Title,
                    Message = vm.Error,
                    IsError = true
                });
            }

            foreach (ContentRenderVM<ObjectId> item in vm.ContentRenderVMs)
            {
                item.Url = SetFullUrl(SiteConstants.URL_BASE_EDIT_POST, item.Content.ContentID.ToString());
            }

            return View(vm);
        }
        
        public async Task<ActionResult> Add()
        {
            ObjectId contentID = ObjectId.GenerateNewId();
            List<string> userRoles = _userRoles.Select(p => p.ToString()).ToList();

            QueryResult<List<Category<ObjectId>>> categoriesResult =
                await _postManager.CategoryManager.SelectIncluded((int)CategoryPermission.Insert, userRoles);
            
            return View("Edit", new ContentEditVM<ObjectId>()
            {
                Content = new Post<ObjectId>()
                {
                    PublishTimeUtc = DateTime.UtcNow,
                    IsPublished = true,
                    ContentID = contentID
                },
                ContentID = _contentEncryption.Encrypt(contentID.ToString()),
                Error = categoriesResult.HasExceptions ? MessageResources.Common_DatabaseException : null,
                Categories = categoriesResult.Result.ToSelectListItems(),              
                IsEditPage = false,
                PreviewImageSizeLimit = _postManager.PreviewImageQueries.Settings.SizeLimit
            });
        }

        [HttpPost]
        public async Task<ActionResult> Add(ContentSubmitVM contentVM,
            [Bind(Include = "Title,FullContent,ShortContent,CategoryID,PublishTimeUtc,IsPublished")]
            Post<ObjectId> content)
        {
            contentVM.ContentID = _contentEncryption.Decrypt(contentVM.ContentID);
            content.PublishTimeUtc = content.PublishTimeUtc.ToUniversalTime();
            content.FullContent = HttpUtility.HtmlDecode(content.FullContent);
            content.ShortContent = HttpUtility.HtmlDecode(content.ShortContent);
            content.ContentID = new ObjectId(contentVM.ContentID);
            content.AuthorID = _currentUser.Id;
            content.AuthorName = _currentUser.UserName;

            List<string> userRoles = _userRoles.Select(p => p.ToString()).ToList();

            ContentEditVM<ObjectId> result = await _postManager.Insert(
                contentVM, content, (int)CategoryPermission.Insert, userRoles);
                        
            if (string.IsNullOrEmpty(result.Error))
            {
                return RedirectToAction("Edit", null, new { id = content.ContentID } );
            }
            else
            {
                result.ContentID = _contentEncryption.Encrypt(contentVM.ContentID);
                return View("Edit", result);
            }
        }
        
        public async Task<ActionResult> Edit(ObjectId id)
        {
            List<string> userRoles = _userRoles.Select(p => p.ToString()).ToList();

            ContentEditVM<ObjectId> result = 
                await _postManager.SelectToEdit(id, (int)CategoryPermission.Edit, userRoles);
           
            if(result.UpdateResult == ContentUpdateResult.NotFound
                || result.UpdateResult == ContentUpdateResult.PermissionException)
            {
                return View("_Message", new MessageContentVM()
                {
                    Title = GlobalContent.Post_Edit_Title,
                    Message = result.Error,
                    IsError = true
                });
            }

            return View(result);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(ObjectId id, ContentSubmitVM contentVM,
            [Bind(Include = "Title,FullContent,ShortContent,CategoryID,PublishTimeUtc,IsPublished")]
            Post<ObjectId> content)
        {
            content.PublishTimeUtc = content.PublishTimeUtc.ToUniversalTime();
            content.FullContent = HttpUtility.HtmlDecode(content.FullContent);
            content.ShortContent = HttpUtility.HtmlDecode(content.ShortContent);
            content.ContentID = id;
            content.AuthorID = _currentUser.Id;
            content.AuthorName = _currentUser.UserName;
            
            List<string> userRoles = _userRoles.Select(p => p.ToString()).ToList();

            ContentEditVM<ObjectId> result = 
                await _postManager.Update(contentVM, content, (int)CategoryPermission.Edit, userRoles);
            return View(result);
        }

        public ActionResult Delete()
        {
            return View("_Message", new MessageContentVM()
            {
                Title = GlobalContent.Post_Delete_Title,
                Message = GlobalContent.Post_Delete_Message,
                IsError = false
            });
        }

        [HttpPost]
        public async Task<JsonResult> Delete(ObjectId contentID)
        {
            List<string> userRoles = _userRoles.Select(p => p.ToString()).ToList();

            PipelineResult result = await _postManager.Delete(contentID, (int)CategoryPermission.Delete, userRoles);

            return Json(new
            {
                result = result.Result,
                error = result.Message
            });
        }

        

        //images
        [HttpPost]
        public async Task<ActionResult> UploadContentImage()
        {
            HttpPostedFileBase file = Request.Files.Count > 0 ? Request.Files[0] : null;
            string userID = _currentUser.Id.ToString();

            PipelineResult<List<ImagePipelineResult>> result = 
                await _postManager.ContentImageQueries.CreateTempImage(file, userID);
            
            return result.Result
                ? Json(new
                {
                    uploaded = 1,
                    fileName = file.FileName,
                    url = result.Content[0].Url
                })
                : Json(new
                {
                    uploaded = 0,
                    error = new
                    {
                        message = result.Message
                    }
                });
        }
        
        [HttpPost]
        public async Task<ActionResult> UploadPreviewImage()
        {
            HttpPostedFileBase file = Request.Files.Count > 0 ? Request.Files[0] : null;
            string userID = _currentUser.Id.ToString();

            PipelineResult<List<ImagePipelineResult>> result =
               await _postManager.PreviewImageQueries.CreateTempImage(file, userID);
            
            return result.Result
                ? Json(new
                {
                    url = result.Content.First().Url,
                    fileID = result.Content.First().FileID
                })
                : Json(new
                {
                    error = result.Message
                });
        }
        
        [HttpPost]
        public async Task<ActionResult> DownloadPreviewImageFromUrl(string fileID, string url)
        {
            string userID = _currentUser.Id.ToString();

            PipelineResult<List<ImagePipelineResult>> result =
               await _postManager.PreviewImageQueries.CreateTempImage(url, userID);

            return result.Result
                ? Json(new
                {
                    url = result.Content.First().Url,
                    fileID = result.Content.First().FileID
                })
                : Json(new
                {
                    error = result.Message
                });
        }
        
    }
}