using ContentManagementBackend;
using ContentManagementBackend.AmazonS3Files;
using ContentManagementBackend.InitializerModules.Resources;
using ContentManagementBackend.MongoDbStorage;
using Common.DataGenerator;
using Common.MongoDb;
using Common.Utility;
using Common.Utility.Pipelines;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Initializer;

namespace ContentManagementBackend.InitializerModules
{
    public class DemoContentModule : IInitializeModule
    {
        //поля
        private ICommonLogger _logger;
        private PreviewImageQueries _previewQueries;
        private DemoContentSettings _settings;
        private IFileStorage _fileStorage;
        private MongoDbContext _context;
        private List<PostEssentials> _posts;
        private List<UserEssentials> _users;
        private int _commentsPerPostCount = 5;


        //события
        public event ProgressDelegate ProgressUpdated;



        //инициализация
        public DemoContentModule(ICommonLogger logger
            , PreviewImageQueries previewImageQueries, IFileStorage fileStorage
            , MongoDbConnectionSettings mongoSettings, DemoContentSettings settings)
        {
            _logger = logger;
            _previewQueries = previewImageQueries;
            _fileStorage = fileStorage;
            _settings = settings;
            _context = new MongoDbContext(mongoSettings);
        }

        
        //методы
        public string IntroduceSelf()
        {
            Category<ObjectId> category = SelectCategory();
            string categoryName = category == null
                ? InnerMessages.CategoryMissing
                : string.Format("{0}: {1}", InnerMessages.CategoryFound, category.Name);

            return "Create demo content. " + categoryName;
        }

        public async Task Execute()
        {
            Category<ObjectId> category = SelectCategory();
            if (category == null)
            {
                _logger.Error(InnerMessages.CategoryMissing);
                return;
            }

            _users = ContentProvider.GetUsers();
            _posts = ContentProvider.GetPosts(_settings.PostContentPath);

            FillTaskAssembler fillTask = new FillTaskAssembler()
                .RegisterSingleResult<ContentBase<ObjectId>>(_settings.PostCount, _context.Posts, CreatePost)
                .RegisterSingleResult<Comment<ObjectId>, ContentBase<ObjectId>>(_commentsPerPostCount, _context.Comments, CreateComment);

            fillTask.Progress.ProgressUpdateEvent += Progress_ProgressUpdateEvent;
            FillResult result = await fillTask.Fill();

            if (result.Exceptions.Count > 0)
            {
                throw new AggregateException(result.Exceptions);
            }

            fillTask.Progress.ProgressUpdateEvent -= Progress_ProgressUpdateEvent;
        }

        private void Progress_ProgressUpdateEvent(decimal obj)
        {
            if (ProgressUpdated != null)
                ProgressUpdated(string.Format(InnerMessages.ProgressPercent, obj));
        }

        private Category<ObjectId> SelectCategory()
        {
            return _context.Categories
                .Find(p => p.CategoryID == _settings.CategoryID)
                .FirstOrDefaultAsync().Result;
        }



        //generators
        private ContentBase<ObjectId> CreatePost(GenerationContext context)
        {
            int postIndex = context.TotalEntityCounter % _posts.Count;
            int listIteration = context.TotalEntityCounter / _posts.Count;

            string title = listIteration + " " + _posts[postIndex].Title;
            string url = Translit.RussianToTranslitUrl(title);
            DateTime publishTime = DateTime.UtcNow.AddDays(-1).AddMinutes(context.TotalEntityCounter);
            UserEssentials user = RandomHelper.PickFromList(_users);

            if (_settings.CreatePreviewImages)
            {
                CreatePostPreviewImage(url, _posts[postIndex].ImageFile);
            }

            if(_posts[postIndex].ShortContent == null)
            {
                _posts[postIndex].ShortContent =
                    ContentMinifier.Minify(_posts[postIndex].FullContent, 250, ContentMinifyMode.ToClosestDot);
            }

            var post = new Post<ObjectId>()
            {
                ContentID = ObjectId.GenerateNewId(),
                CategoryID = _settings.CategoryID,
                AuthorID = user.ID,
                AuthorName = user.Name,
                Title = title,
                Url = url,
                CommentsCount = _commentsPerPostCount,
                ViewsCount = 0,
                FullContent = _posts[postIndex].FullContent,
                ShortContent = _posts[postIndex].ShortContent,
                HasImage = true,
                IsPublished = true,
                AddTimeUtc = publishTime,
                PublishTimeUtc = publishTime,
                IsIndexed = _settings.MarkAsIndexedInSearch
            };

            post.CreateUpdateNonce();
            
            return post;
        }

        private void CreatePostPreviewImage(string url, FileInfo imageFile)
        {
            var pipeline = new UploadImagePipeline(_fileStorage);
            
            PipelineResult result = pipeline.Process(new ImagePipelineModel()
            {
                InputStream = imageFile.OpenRead(),
                ContentLength = 0,              
                SizeLimit = int.MaxValue,
                Targets = new List<ImageTargetParameters>()
                {
                    new ImageTargetParameters()
                    {
                        TargetFormat = ImageFormat.Jpeg,
                        RoundCorners = true,
                        Width = 750,
                        Height = 420,
                        ResizeType = ImageResizeType.FitAndFill
                    }                  
                },
                TargetNamePaths = new List<string>()
                {
                    _previewQueries.PathCreator.CreateStaticNamePath(url)
                }
            }).Result;

            if(!result.Result)
            {
                throw new Exception(result.Message);
            }
        }

        private Comment<ObjectId> CreateComment(GenerationContext<ContentBase<ObjectId>> context)
        {
            ObjectId currentID = ObjectId.GenerateNewId();
            ObjectId? branchCommentID;
            ObjectId? parentCommentID;
            DateTime publishTime = DateTime.UtcNow.AddDays(-1).AddMinutes(context.TotalEntityCounter);
            
            switch (context.ItemsPerForeignEntityCounter)
            {
                case 0:
                    branchCommentID = null;
                    parentCommentID = null;
                    break;
                case 1:
                    branchCommentID = null;
                    parentCommentID = null;
                    context.Bag["branchCommentID"] = currentID;
                    break;
                default:
                    branchCommentID = (ObjectId)context.Bag["branchCommentID"];
                    parentCommentID = (ObjectId)context.Bag["lastCommentID"];
                    break;
            }

            context.Bag["lastCommentID"] = currentID;
            UserEssentials user = RandomHelper.PickFromList(_users);

            return new Comment<ObjectId>()
            {
                ContentID = context.ForeignEntity.ContentID,
                CommentID = currentID,
                BranchCommentID = branchCommentID,
                ParentCommentID = branchCommentID,
                AddTimeUtc = publishTime,
                UserUpdateTimeUtc = publishTime,
                Content = "Comment content",
                DownVotes = 0,
                UpVotes = 0,
                State = CommentState.Inserted,
                AuthorID = user.ID,
                AuthorAvatar = user.Avatar,
                AuthorName = user.Name
            };
        }
    }
}
