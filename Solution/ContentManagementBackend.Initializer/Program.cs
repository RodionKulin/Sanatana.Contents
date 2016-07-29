using ContentManagementBackend.AmazonS3Files;
using ContentManagementBackend.Demo;
using ContentManagementBackend.ElasticSearch;
using ContentManagementBackend.MongoDbStorage;
using ContentManagementBackend.InitializerModules;
using Common.MongoDb;
using Common.Utility;
using MongoDB.Bson;
using MongoDB.Driver;
using Nest;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Autofac;
using Common.Identity2_1.MongoDb;
using Microsoft.AspNet.Identity;
using Common.Initializer;
using System.Threading;

namespace ContentManagementBackend.Initializer
{
    class Program
    {
        static void Main(string[] args)
        {
            MongoDbContext.ApplyGlobalSerializationSettings();
            Common.MongoDb.ObjectIdTypeConverter.Register();
            
            RunInitializeModules();
        }

        private static void RunInitializeModules()
        {
            var initializer = new InitializeManager();

            RegisterServices(initializer);
            RegisterImageParameters(initializer);
            RegisterParameters(initializer);
                        
            initializer.RegisterModules(new List<Type>()
            {
                typeof(DropMongoDbModule),
                typeof(CreateMongoDbIndexModule<UserAccount>),
                //typeof(ESInstallModule),
                typeof(CategoriesModule),
                typeof(AdminUserModule),
                typeof(DemoContentModule),
                //typeof(ESInsertDataModule),
                typeof(ManualsModule),
                //typeof(AvatarsModule),
            });

            initializer.Initialize();
        }

        private static void RegisterServices(InitializeManager initializer)
        {
            initializer.Builder.RegisterType<ShoutExceptionLogger>().As<ICommonLogger>();

            initializer.Builder.RegisterType<ESQueries<ObjectId>>().As<ISearchQueries<ObjectId>>();
            initializer.Builder.RegisterType<MongoDbCategoryQueries>().As<ICategoryQueries<ObjectId>>();
            initializer.Builder.RegisterType<MongoDbContentQueries>().As<IContentQueries<ObjectId>>();

            initializer.Builder.RegisterType<IdentityMongoContext<UserAccount>>().AsSelf();
            initializer.Builder.RegisterType<MongoUserStore<UserAccount>>().As<IUserStore<UserAccount, ObjectId>>();
            initializer.Builder.RegisterType<MongoUserQueries<UserAccount>>().AsSelf();
            initializer.Builder.RegisterType<MongoUserManager>().AsSelf();

            initializer.Builder.RegisterType<AmazonFileStorage>().As<IFileStorage>();
            initializer.Builder.RegisterType<ImageSettingsFactory>().As<IImageSettingsFactory>();
            initializer.Builder.RegisterType<ContentImageQueries>().AsSelf();
            initializer.Builder.RegisterType<PreviewImageQueries>().AsSelf();
            initializer.Builder.RegisterType<AvatarImageQueries>().AsSelf();
        }

        private static void RegisterImageParameters(InitializeManager initializer)
        {
            initializer.Builder.RegisterInstance(new AvatarImageSettings()
            {
                DefaultImages = ContentProvider.GetAvatarNames(),
                TempDeleteAge = TimeSpan.FromDays(1),
                SizeLimit = 1124000,
                Targets = new List<ImageTargetParameters>()
                {
                    new ImageTargetParameters()
                    {
                        TargetFormat = ImageFormat.Jpeg,
                        ResizeType = ImageResizeType.FitAndFill,
                        RoundCorners = false,
                        Height = 100,
                        Width = 100,
                        PathCreator = new PathCreator()
                        {
                            TempRootFolder = Constants.IMAGE_TEMP_FOLDER_AVATAR,
                            TempFolderFormat = null,
                            TempNameFormat = "{0}{1}.jpeg",
                            StaticRootFolder = Constants.IMAGE_STATIC_FOLDER_AVATAR,
                            StaticFolderFormat = null,
                            StaticNameFormat = "{0}.jpeg"
                        }
                    }
                }
            });

            initializer.Builder.RegisterInstance(new ImageSettings()
            {
                Name = Constants.IMAGE_SETTINGS_NAME_PREVIEW,
                TempDeleteAge = TimeSpan.FromDays(1),
                SizeLimit = 5124000,
                Targets = new List<ImageTargetParameters>()
                {
                    new ImageTargetParameters()
                    {
                        TargetFormat = ImageFormat.Jpeg,
                        ResizeType = ImageResizeType.FitAndFill,
                        RoundCorners = false,
                        Width = 750,
                        Height = 420,
                        PathCreator = new PathCreator()
                        {
                            TempRootFolder = Constants.IMAGE_TEMP_FOLDER_PREVIEW,
                            TempFolderFormat = null,
                            TempNameFormat = "{0}{1}.jpeg",
                            StaticRootFolder = Constants.IMAGE_STATIC_FOLDER_PREVIEW,
                            StaticFolderFormat = null,
                            StaticNameFormat = "{0}.jpeg"
                        }
                    }
                }
            });

            initializer.Builder.RegisterInstance(new ImageSettings()
            {
                Name = Constants.IMAGE_SETTINGS_NAME_CONTENT,
                TempDeleteAge = TimeSpan.FromDays(1),
                SizeLimit = 1124000,
                Targets = new List<ImageTargetParameters>()
                {
                    new ImageTargetParameters()
                    {
                        TargetFormat = ImageFormat.Jpeg,
                        ResizeType = ImageResizeType.FitRatio,
                        RoundCorners = false,
                        Width = 600,
                        Height = 600,
                        PathCreator = new PathCreator()
                        {
                            TempRootFolder = Constants.IMAGE_TEMP_FOLDER_CONTENT,
                            TempFolderFormat = null,
                            TempNameFormat = "{0}{1}.jpeg",
                            StaticRootFolder = Constants.IMAGE_STATIC_FOLDER_CONTENT,
                            StaticFolderFormat = "{0}",
                            StaticNameFormat = "{0}.jpeg"
                        }
                    }
                }
            });
        }

        private static void RegisterParameters(InitializeManager initializer)
        {
            //data
            List<Category<ObjectId>> categories = CategoriesProvider.GetCategories();
            Category<ObjectId> manualsCategory = categories.First(p =>
                p.Permissions.Any(a => a.Key == (int)CategoryPermission.View && a.Value == IdentityUserRole.Author.ToString()));

            var admin = new UserEssentials()
            {
                ID = ObjectId.GenerateNewId(),
                Email = "master@cmb.ru",
                Name = "master",
                Password = "password1",
                Roles = new List<string>() { IdentityUserRole.Author.ToString(), IdentityUserRole.Moderator.ToString(), IdentityUserRole.Admin.ToString() }
            };
            var author = new UserEssentials()
            {
                ID = ObjectId.GenerateNewId(),
                Email = "author1@cmb.ru",
                Name = "author1",
                Password = "password1",
                Roles = new List<string>() { IdentityUserRole.Author.ToString() }
            };

            var mongoConnection = MongoDbConnectionSettings.FromConfig();
            var esConnection = ESSettings.FromConfig();
            var amazonConnection = AmazonS3Settings.FromConfig();
            initializer.RegisterPrintable(mongoConnection, mongoConnection.ToDetailsString());
            initializer.RegisterPrintable(esConnection, esConnection.ToDetailsString());
            initializer.RegisterPrintable(amazonConnection, amazonConnection.ToDetailsString());

            initializer.Builder.RegisterInstance(new CategoriesSettings()
            {
                Categories = categories
            });
            initializer.Builder.RegisterInstance(new DemoContentSettings()
            {
                CreatePreviewImages = false,
                MarkAsIndexedInSearch = true,
                CategoryID = categories.First(p => p.ParentCategoryID != null).CategoryID,
                PostCount = 50,
                PostContentPath = "Content\\demo posts"
            });
            initializer.Builder.RegisterInstance(new AdminModuleSettings()
            {
                Users = new List<UserEssentials>()
                {
                    admin,
                    author
                }
            });
            initializer.Builder.RegisterInstance(new ManualsSettings()
            {
                CategoryID = manualsCategory.CategoryID,
                PostContentPath = "Content\\manual posts",
                Author = admin
            });
        }



    }
}
