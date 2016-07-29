using ContentManagementBackend.MongoDbStorage;
using Common.MongoDb;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using MongoDB.Driver;
using Common.Utility;
using ContentManagementBackend.InitializerModules.Resources;
using Common.Initializer;
using Common.Utility.Pipelines;

namespace ContentManagementBackend.InitializerModules
{
    public class ManualsModule : IInitializeModule
    {
        //поля
        private ICommonLogger _logger;
        private MongoDbContext _context;
        private ManualsSettings _settings;
        private PreviewImageQueries _previewQueries;
        private ContentImageQueries _contentQueries;


        //события
        public event ProgressDelegate ProgressUpdated;



        //инициализация
        public ManualsModule(ICommonLogger logger
            , PreviewImageQueries previewImageQueries, ContentImageQueries contentQueries
            , ManualsSettings settings, MongoDbConnectionSettings mongoSettings)
        {
            _logger = logger;
            _previewQueries = previewImageQueries;
            _contentQueries = contentQueries;
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

            return "Insert manual posts. " + categoryName;
        }

        public async Task Execute()
        {
            Category<ObjectId> category = SelectCategory();
            if (category == null)
            {
                _logger.Error(InnerMessages.CategoryMissing);
                return;
            }

            List<Post<ObjectId>> posts = new List<Post<ObjectId>>();
            List<PostEssentials> postEssencials = ContentProvider.GetPosts(_settings.PostContentPath);

            foreach (PostEssentials item in postEssencials)
            {
                if (item.ShortContent == null)
                {
                    item.ShortContent =
                        ContentMinifier.Minify(item.FullContent, 250, ContentMinifyMode.ToClosestDot);
                }

                var post = new Post<ObjectId>()
                {
                    ContentID = ObjectId.GenerateNewId(),
                    CategoryID = _settings.CategoryID,
                    AuthorID = _settings.Author.ID,
                    AuthorName = _settings.Author.Name,
                    Title = item.Title,
                    Url = Translit.RussianToTranslitUrl(item.Title),
                    CommentsCount = 0,
                    ViewsCount = 0,
                    FullContent = item.FullContent,
                    ShortContent = item.ShortContent,
                    HasImage = true,
                    IsPublished = true,
                    AddTimeUtc = DateTime.UtcNow,
                    PublishTimeUtc = DateTime.UtcNow,
                    IsIndexed = false
                };
                post.CreateUpdateNonce();
                posts.Add(post);

                if (item.ImageFile != null)
                {
                    using (Stream file = item.ImageFile.OpenRead())
                    {
                        PipelineResult imageResult = await _previewQueries.CreateStaticImage(file, post.Url);
                    }
                }

                await UploadContentImages(post);
            }

            var insertOptions = new InsertManyOptions() { IsOrdered = false };
            await _context.Posts.InsertManyAsync(posts, insertOptions);

        }

        private Category<ObjectId> SelectCategory()
        {
            return _context.Categories
                .Find(p => p.CategoryID == _settings.CategoryID)
                .FirstOrDefaultAsync().Result;
        }

        private async Task UploadContentImages(Post<ObjectId> post)
        {
            var fullContent = HtmlParser.Parse(post.FullContent);
            var shortContent = HtmlParser.Parse(post.ShortContent);
            List<HtmlElement> content = new List<HtmlElement>()
            {
                fullContent,
                shortContent
            };

            foreach (HtmlElement document in content)
            {
                List<HtmlImageInfo> images = HtmlMediaExtractor.FindImages(document);

                foreach (HtmlImageInfo image in images)
                {
                    string fileUri = image.Src.Replace("http://", "");
                    fileUri = Path.Combine(_settings.PostContentPath, fileUri);

                    using (Stream fileStream = File.OpenRead(fileUri))
                    {
                        string[] pathParts = new [] {
                            post.ContentID.ToString(),
                            ShortGuid.NewGuid().ToString()
                        };
                        PipelineResult<List<ImagePipelineResult>> imageResult = 
                            await _contentQueries.CreateStaticImage(fileStream, pathParts);

                        image.Src = imageResult.Content[0].Url;
                    }

                }
            }

            post.FullContent = fullContent.ToString();
            post.ShortContent = shortContent.ToString();

        }


    }

   
}
