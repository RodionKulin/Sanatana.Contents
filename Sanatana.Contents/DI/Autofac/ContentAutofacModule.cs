using Autofac;
using Sanatana.Contents.Database;
using Sanatana.Contents.Html.Media;
using Sanatana.Contents.Pipelines;
using Sanatana.Contents.Pipelines.Categories;
using Sanatana.Contents.Pipelines.Comments;
using Sanatana.Contents.Pipelines.Contents;
using Sanatana.Contents.Pipelines.Images;
using Sanatana.Contents.Selectors.Categories;
using Sanatana.Contents.Selectors.Comments;
using Sanatana.Contents.Selectors.Contents;
using Sanatana.Contents.Selectors.Permissions;
using Sanatana.Contents.Utilities;

namespace Sanatana.Contents.DI.Autofac
{
    public class ContentAutofacModule : Module
    {
        //methods
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EntitiesDatabaseNameMapping>().As<IEntitiesDatabaseNameMapping>().SingleInstance();
            builder.RegisterType<RethrowPipelineExceptionHandler>().As<IPipelineExceptionHandler>().InstancePerDependency();
            builder.RegisterType<HtmlMediaExtractor>().As<IHtmlMediaExtractor>().InstancePerDependency();
            builder.RegisterType<UrlEncoder>().As<IUrlEncoder>().InstancePerDependency();

            builder.RegisterGeneric(typeof(InsertCategoryPipeline<,>)).As(typeof(IInsertCategoryPipeline<,>)).InstancePerDependency();
            builder.RegisterGeneric(typeof(UpdateCategoryPipeline<,>)).As(typeof(IUpdateCategoryPipeline<,>)).InstancePerDependency();
            builder.RegisterGeneric(typeof(DeleteCategoryPipeline<,>)).As(typeof(IDeleteCategoryPipeline<,>)).InstancePerDependency();

            builder.RegisterGeneric(typeof(InsertContentPipeline<,,>)).As(typeof(IInsertContentPipeline<,,>)).InstancePerDependency();
            builder.RegisterGeneric(typeof(UpdateContentPipeline<,,>)).As(typeof(IUpdateContentPipeline<,,>)).InstancePerDependency();
            builder.RegisterGeneric(typeof(DeleteContentPipeline<,,,>)).As(typeof(IDeleteContentPipeline<,,,>)).InstancePerDependency();

            builder.RegisterGeneric(typeof(InsertCommentPipeline<,,,>)).As(typeof(IInsertCommentPipeline<,,,>)).InstancePerDependency();
            builder.RegisterGeneric(typeof(UpdateCommentPipeline<,,,>)).As(typeof(IUpdateCommentPipeline<,,,>)).InstancePerDependency();
            builder.RegisterGeneric(typeof(DeleteCommentPipeline<,,,>)).As(typeof(IDeleteCommentPipeline<,,,>)).InstancePerDependency();

            builder.RegisterType<UploadImagePipeline>().As(typeof(IUploadImagePipeline)).InstancePerDependency();

            builder.RegisterGeneric(typeof(CategorySelector<,>)).As(typeof(ICategorySelector<,>)).InstancePerDependency();
            builder.RegisterGeneric(typeof(ContentSelector<,,>)).As(typeof(IContentSelector<,,>)).InstancePerDependency();
            builder.RegisterGeneric(typeof(CommentSelector<,,,>)).As(typeof(ICommentSelector<,,,>)).InstancePerDependency();
            builder.RegisterGeneric(typeof(PermissionSelector<,>)).As(typeof(IPermissionSelector<,>)).InstancePerDependency();
        }
    }
}
