using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using MongoDB.Bson;
using Newtonsoft.Json;
using Common.Identity2_1;
using Common.Utility;
using Common.Web;
using ContentManagementBackend;
using ContentManagementBackend.Resources;
using System.Web.Helpers;
using Common.Utility.Pipelines;

namespace ContentManagementBackend.Demo.Controllers
{
    public class CommentsController : BaseController
    {
        //поля
        private AuthManager _authManager;



        //инициализация
        public CommentsController(CustomContentManager contentManager, AuthManager authManager)
            : base(contentManager)
        {
            _postManager = contentManager;
            _authManager = authManager;
        }



        //методы
        [HttpPost]
        public async Task<ActionResult> List(ObjectId contentID, ObjectId categoryID)
        {
            List<string> userRoles = _userRoles.Select(p => p.ToString()).ToList();

            MessageResult<List<CommentRenderVM<ObjectId>>> comments = 
                await _postManager.CommentManager.SelectForSubject(
                contentID, categoryID, (int)CategoryPermission.View, userRoles);

            return new JsonCamelCaseResult
            {
                Data = new
                {
                    error = comments.HasExceptions ? comments.Message : null,
                    list = comments.Result
                },
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        [HttpPost]
        public async Task<ActionResult> Add(CommentAuthPipelineModel authVM, CommentVM<ObjectId> commentVM)
        {
            AuthResult authResult = null;

            //Auth
            if (!User.Identity.IsAuthenticated)
            {
                authResult = await _authManager.AuthComment(authVM);
                if (!authResult.Result)
                {
                    return Json(new
                    {
                        message = authResult.Message
                    });
                }
                _currentUser = authResult.User;
            }

            //Add comment
            commentVM.Content = HttpUtility.HtmlDecode(commentVM.Content);
            var comment = new Comment<ObjectId>(commentVM);
            comment.AuthorID = _currentUser.Id;
            comment.AuthorName = _currentUser.UserName;
            comment.AuthorAvatar = _currentUser.Avatar;

            PipelineResult result = await _postManager.CommentManager.Insert(new CommentPipelineModel<ObjectId>()
            {
                Comment = comment,
                UserRoles = _userRoles.Select(p => p.ToString()).ToList(),
                Permission = (int)CategoryPermission.View,
                ViewPermission = (int)CategoryPermission.View
            });

            return new JsonCamelCaseResult()
            {
                Data = new
                {
                    message = result.Message,
                    comment = result.Result
                        ? new CommentRenderVM<ObjectId>(comment, _postManager.AvatarImageQueries)
                        : null,
                    user = authResult == null
                        ? null
                        : new
                        {
                            userName = authResult.User.UserName,
                            avatar = _postManager.AvatarImageQueries.PathCreator.CreateStaticUrl(authResult.User.Avatar),
                            antiForgeryToken = authResult.AntiForgeryToken
                        }
                },
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        [HttpPost]
        public async Task<ActionResult> Update(CommentVM<ObjectId> commentVM)
        {
            //?userID match

            commentVM.Content = HttpUtility.HtmlDecode(commentVM.Content);
            var comment = new Comment<ObjectId>(commentVM);
            comment.CommentID = commentVM.CommentID ?? ObjectId.Empty;
            comment.AuthorID = _currentUser.Id;
            comment.AuthorName = User.Identity.Name;
            comment.AuthorAvatar = User.GetAvatar();

            PipelineResult result = await _postManager.CommentManager.Update(new CommentPipelineModel<ObjectId>()
            {
                Comment = comment,
                UserRoles = _userRoles.Select(p => p.ToString()).ToList(),
                Permission = (int)CategoryPermission.View,
                ViewPermission = (int)CategoryPermission.View,
            });

            return new JsonCamelCaseResult
            {
                Data = new
                {
                    error = result.Result ? null : result.Message,
                    list = result.Result
                        ? new CommentRenderVM<ObjectId>(comment, _postManager.AvatarImageQueries)
                        : null,
                    state = comment.State
                },
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        [HttpPost]
        public async Task<ActionResult> Delete(CommentVM<ObjectId> commentVM)
        {
            //?userID match

            var comment = new Comment<ObjectId>(commentVM);
            comment.CommentID = commentVM.CommentID ?? ObjectId.Empty;
            comment.AuthorID = _currentUser.Id;
            comment.AuthorName = _currentUser.UserName;
            comment.AuthorAvatar = _currentUser.Avatar;

            PipelineResult result = await _postManager.CommentManager.Delete(new CommentPipelineModel<ObjectId>()
            {
                Comment = comment,
                UserRoles = _userRoles.Select(p => p.ToString()).ToList(),
                Permission = (int)CategoryPermission.View,
                ViewPermission = (int)CategoryPermission.View,
            });

            return new JsonCamelCaseResult
            {
                Data = new
                {
                    error = result.Result ? null : result.Message,
                    result = result.Result
                }
            };
        }



        //images
        [HttpPost]
        public async Task<ActionResult> UploadCommentImage()
        {
            HttpPostedFileBase file = Request.Files.Count > 0 ? Request.Files[0] : null;
            string userID = _currentUser.Id.ToString();

            PipelineResult<List<ImagePipelineResult>> result =
                await _postManager.CommentImageQueries.CreateTempImage(file, userID);

            return result.Result
                ? Json(new
                {
                    uploaded = 1,
                    fileName = file.FileName,
                    url = result.Content.First().Url
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
    }
}