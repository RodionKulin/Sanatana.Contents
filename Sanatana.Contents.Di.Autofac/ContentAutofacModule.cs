using Autofac;
using Sanatana.Contents.Database;
using Sanatana.Contents.Html.Media;
using Sanatana.Contents.Pipelines;
using Sanatana.Contents.Pipelines.Categories;
using Sanatana.Contents.Pipelines.Comments;
using Sanatana.Contents.Pipelines.Contents;
using Sanatana.Contents.Pipelines.Images;
using Sanatana.Contents.Search;
using Sanatana.Contents.Selectors.Categories;
using Sanatana.Contents.Selectors.Comments;
using Sanatana.Contents.Selectors.Contents;
using Sanatana.Contents.Selectors.Permissions;
using Sanatana.Contents.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sanatana.Contents.Di.Autofac
{
    public class ContentAutofacModule<TKey> : Module
        where TKey : struct
    {
        //methods
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EntitiesDatabaseNameMapping>().AsSelf().SingleInstance();
            builder.RegisterType<RethrowPipelineExceptionHandler>().As<IPipelineExceptionHandler>().InstancePerDependency();
            
            builder.RegisterGeneric(typeof(InsertCategoryPipeline<,>)).AsSelf().InstancePerDependency();
            builder.RegisterGeneric(typeof(UpdateCategoryPipeline<,>)).AsSelf().InstancePerDependency();
            builder.RegisterGeneric(typeof(DeleteCategoryPipeline<,>)).AsSelf().InstancePerDependency();

            builder.RegisterGeneric(typeof(InsertContentPipeline<,,>)).AsSelf().InstancePerDependency();
            builder.RegisterGeneric(typeof(UpdateContentPipeline<,,>)).AsSelf().InstancePerDependency();
            builder.RegisterGeneric(typeof(DeleteContentPipeline<,,,>)).AsSelf().InstancePerDependency();

            builder.RegisterGeneric(typeof(InsertCommentPipeline<,,,>)).AsSelf().InstancePerDependency();
            builder.RegisterGeneric(typeof(UpdateCommentPipeline<,,,>)).AsSelf().InstancePerDependency();
            builder.RegisterGeneric(typeof(DeleteCommentPipeline<,,,>)).AsSelf().InstancePerDependency();

            builder.RegisterType<UploadImagePipeline>().AsSelf().InstancePerDependency();

            builder.RegisterGeneric(typeof(CategorySelector<,>)).As(typeof(ICategorySelector<,>)).InstancePerDependency();
            builder.RegisterGeneric(typeof(ContentSelector<,,>)).As(typeof(IContentSelector<,,>)).InstancePerDependency();
            builder.RegisterGeneric(typeof(CommentSelector<,,,>)).As(typeof(ICommentSelector<,,,>)).InstancePerDependency();
            builder.RegisterGeneric(typeof(PermissionSelector<,>)).As(typeof(IPermissionSelector<,>)).InstancePerDependency();

            builder.RegisterType<HtmlMediaExtractor>().As<IHtmlMediaExtractor>().InstancePerDependency();
            builder.RegisterType<UrlEncoder>().As<IUrlEncoder>().InstancePerDependency();


        }
    }
}
