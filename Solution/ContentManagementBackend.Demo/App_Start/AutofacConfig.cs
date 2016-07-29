using Owin;
using Autofac;
using Autofac.Integration.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Web.Mvc;
using MongoDB.Bson;
using System.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security;
using ContentManagementBackend.MongoDbStorage;
using ContentManagementBackend.ElasticSearch;
using ContentManagementBackend.Demo;
using Common.Utility;
using Autofac.Core;
using Common.MongoDb;
using System.Drawing.Imaging;
using MongoDB.Driver;
using ContentManagementBackend.AmazonS3Files;
using Common.Identity2_1.MongoDb;

namespace ContentManagementBackend.Demo
{
    public class AutofacConfig
    {
        public static void Configure(IAppBuilder app)
        {
            IContainer container = BuildAutofacContainer(app);

            // Set the dependency resolver to be Autofac.
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // Register the Autofac middleware FIRST, then the Autofac MVC middleware.
            app.UseAutofacMiddleware(container);
            app.UseAutofacMvc();            
        }

        private static IContainer BuildAutofacContainer(IAppBuilder app)
        {
            var builder = new ContainerBuilder();

            // Register your MVC controllers.
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // OPTIONAL: Register model binders that require DI.
            builder.RegisterModelBinders(Assembly.GetExecutingAssembly());
            builder.RegisterModelBinderProvider();

            // OPTIONAL: Register web abstractions like HttpContextBase.
            builder.RegisterModule<AutofacWebTypesModule>();

            // OPTIONAL: Enable property injection in view pages.
            builder.RegisterSource(new ViewRegistrationSource());

            // OPTIONAL: Enable property injection into action filters.
            builder.RegisterFilterProvider();

            //Register Dependencies
            RegisterIdentityParts(app, builder);
            RegisterImageParts(app, builder);
            RegisterContentParts(app, builder);

            return builder.Build();            
        }

        private static void RegisterIdentityParts(IAppBuilder app, ContainerBuilder builder)
        {
            builder.RegisterType<IdentityMongoContext<UserAccount>>().AsSelf().SingleInstance();
            
            builder.Register<IAuthenticationManager>(c => HttpContext.Current.GetOwinContext().Authentication)
                .InstancePerRequest();
            builder.Register<IDataProtectionProvider>(c => app.GetDataProtectionProvider())
                .InstancePerRequest();

            builder.RegisterType<MongoUserStore<UserAccount>>().As<IUserStore<UserAccount, ObjectId>>()
                .InstancePerRequest();

            builder.Register<MongoUserManager>(c =>
                MongoUserManager.Create(c.Resolve<IUserStore<UserAccount, ObjectId>>()
                , c.Resolve<IDataProtectionProvider>(), c.Resolve<ICommonLogger>()))
                .InstancePerRequest();
            builder.RegisterType<MongoSignInManager>().AsSelf().InstancePerRequest();
            
            builder.RegisterType<IdentityQueries>().As<IIdentityQueries>();
            builder.RegisterType<AuthManager>().AsSelf().InstancePerRequest();
        }

        private static void RegisterImageParts(IAppBuilder app, ContainerBuilder builder)
        {
            AmazonS3Settings amazonSettings = AmazonS3Settings.FromConfig();
            ICommonLogger logger = new NLogLogger();
            IFileStorage fileStorage = new AmazonFileStorage(logger, amazonSettings);

            var avatarImageSettings = new AvatarImageSettings()
            {
                DefaultImages = new List<string>() {
                    "3ysYtgscfkekTqi5bS6XHw", "5eJ-trR5WUGEQZ49rzj2Kw", "cnGYOdKfX0iUKv1fmkB6KA"
                    , "dC-HU2RSiUSEliqP_5HmwA", "EFgYkW-LTkSAxLvAE3DsvA", "Gi3Xoqq_4k2ZZy-Aa5vsyw"
                    , "HwFfWSqQgUu5FMrrFvGiag", "Kyc0FjFlI0uQ2QyczDGvrg"
                    , "Oh7YIBQofkubrcY7bs8RBw", "OsHpSXSr6EamPZzL0iwx0g", "pfP-ZQinnkOyZHpKzl-wBg"
                    , "qBCQhTdEtECK559zxocntg", "QcvLm8Mdr0uLCVQv752V9g", "R0T754HITEuKho19rdnGXg"
                    , "SiHl9pLsjUGLau9JZxEY_g", "tQ4JINIZAkq4lR2r3EVjhg", "VUqjCVsxAEmz9Yfcv24Q6A"
                    , "vvt1gG9n30WHhiPhF2KttA", "YEVzYN1iZEKgOqi2LUsHCw", "_PIYJMjqLEShFOuWTAaCwA" },
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
                            UrlBase =  fileStorage.GetBaseUrl(),
                            TempRootFolder = Constants.IMAGE_TEMP_FOLDER_AVATAR,
                            TempFolderFormat = null,
                            TempNameFormat = "{0}{1}.jpeg",
                            StaticRootFolder = Constants.IMAGE_STATIC_FOLDER_AVATAR,
                            StaticFolderFormat = null,
                            StaticNameFormat = "{0}.jpeg"                                
                        }
                    }
                }
            };
          
            var previewImageSettings = new ImageSettings()
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
                            UrlBase = fileStorage.GetBaseUrl(),
                            TempRootFolder = Constants.IMAGE_TEMP_FOLDER_PREVIEW,
                            TempFolderFormat = null,
                            TempNameFormat = "{0}{1}.jpeg",
                            StaticRootFolder = Constants.IMAGE_STATIC_FOLDER_PREVIEW,
                            StaticFolderFormat = null,
                            StaticNameFormat = "{0}.jpeg"
                        }
                    }
                }
            };

            var contentImageSettings = new ImageSettings()
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
                            UrlBase = fileStorage.GetBaseUrl(),
                            TempRootFolder = Constants.IMAGE_TEMP_FOLDER_CONTENT,
                            TempFolderFormat = null,
                            TempNameFormat = "{0}{1}.jpeg",
                            StaticRootFolder = Constants.IMAGE_STATIC_FOLDER_CONTENT,
                            StaticFolderFormat = "{0}",
                            StaticNameFormat = "{0}.jpeg"
                        }
                    }
                }
            };

            var commentImageSettings = new ImageSettings()
            {
                Name = Constants.IMAGE_SETTINGS_NAME_COMMENT,
                TempDeleteAge = TimeSpan.FromDays(1),
                SizeLimit = 5124000,
                Targets = new List<ImageTargetParameters>()
                {
                    new ImageTargetParameters()
                    {
                        TargetFormat = ImageFormat.Jpeg,
                        ResizeType = ImageResizeType.FitRatio,
                        Width = 600,
                        Height = 600,
                        RoundCorners = false,
                        PathCreator = new PathCreator()
                        {
                            UrlBase = fileStorage.GetBaseUrl(),
                            TempRootFolder = Constants.IMAGE_TEMP_FOLDER_COMMENT,
                            TempFolderFormat = null,
                            TempNameFormat = "{0}{1}.jpeg",
                            StaticRootFolder = Constants.IMAGE_STATIC_FOLDER_COMMENT,
                            StaticFolderFormat = "{0}",
                            StaticNameFormat = "{0}.jpeg"
                        }
                    }
                }
            };

            builder.RegisterInstance(fileStorage).As<IFileStorage>();
            builder.RegisterType<ImageSettingsFactory>().As<IImageSettingsFactory>();
            builder.RegisterInstance(avatarImageSettings).AsSelf();
            builder.RegisterInstance(previewImageSettings).AsSelf();
            builder.RegisterInstance(contentImageSettings).AsSelf().PreserveExistingDefaults();
            builder.RegisterInstance(commentImageSettings).AsSelf().PreserveExistingDefaults();
            builder.RegisterType<ContentImageQueries>().AsSelf();
            builder.RegisterType<PreviewImageQueries>().AsSelf();
            builder.RegisterType<AvatarImageQueries>().AsSelf();
            builder.RegisterType<CommentImageQueries>().AsSelf();
        }

        private static void RegisterContentParts(IAppBuilder app, ContainerBuilder builder)
        {           
            string encryptionKey = "dn18fj!dhA)mp";
            
            builder.RegisterType<NLogLogger>().As<ICommonLogger>();
            builder.RegisterType<AspNetCacheProvider>().As<ICacheProvider>();
            builder.RegisterInstance(new ContentEncryption(encryptionKey)).As<IContentEncryption>();
            builder.RegisterType<NoCaptchaProvider>().As<ICaptchaProvider>();
            builder.RegisterType<CommentStateProvider>().As<ICommentStateProvider>();

            builder.RegisterInstance(ESSettings.FromConfig()).AsSelf();
            builder.RegisterType<ESObjectIdQueries>().As<ISearchQueries<ObjectId>>();

            builder.RegisterInstance(MongoDbConnectionSettings.FromConfig()).AsSelf();
            builder.RegisterType<MongoDbContentQueries>().Named<IContentQueries<ObjectId>>("data");
            builder.RegisterType<MongoDbCommentQueries>().Named<ICommentQueries<ObjectId>>("data");
            builder.RegisterType<MongoDbCategoryQueries>().Named<ICategoryQueries<ObjectId>>("data");

            builder.RegisterGenericDecorator(
                typeof(SingleInstancePostCache<>), typeof(IContentQueries<>), fromKey: "data");
            builder.RegisterGenericDecorator(
                typeof(SingleInstanceCommentCache<>), typeof(ICommentQueries<>), fromKey: "data");            
            builder.RegisterGenericDecorator(
                typeof(SingleInstanceCategoryCache<>), typeof(ICategoryQueries<>), fromKey: "data");
            
            builder.RegisterType<CategoryManager<ObjectId>>().As<ICategoryManager<ObjectId>>();
            builder.RegisterType<CommentManager<ObjectId>>().As<ICommentManager<ObjectId>>().InstancePerRequest();
            builder.RegisterType<CustomContentManager>().AsSelf().InstancePerRequest();
        }
    }
}