using Autofac;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Sanatana.Contents.Caching;
using Sanatana.Contents.Caching.CacheProviders;
using Sanatana.Contents.Caching.Concrete;
using Sanatana.Contents.Caching.DataChangeNotifiers;
using Sanatana.Contents.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.DI.Autofac
{
    public class CachingAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            IOptions<MemoryCacheOptions> memoryCacheOptions = Options.Create(new MemoryCacheOptions());
            builder.RegisterInstance(new MemoryCache(memoryCacheOptions)).As<IMemoryCache>().SingleInstance();
            builder.RegisterType<MemoryPersistentCacheProvider>().As<ICacheProvider>().SingleInstance();
            builder.RegisterType<QueryCache>().As<IQueryCache>().SingleInstance();
            builder.RegisterType<DataChangeNotifiersRegistry>().As<IDataChangeNotifiersRegistry>().SingleInstance();

            builder.RegisterGenericDecorator(
                typeof(CategoryFullyCachedQueries<,>), typeof(ICategoryQueries<,>), fromKey: DependencyConstants.AUTOFAC_NAMED_INSTANCE_DEFAULT_KEY)
                .InstancePerLifetimeScope();
            builder.RegisterGenericDecorator(
                typeof(CategoryRolePermissionFullyCachedQueries<>), typeof(ICategoryRolePermissionQueries<>), fromKey: DependencyConstants.AUTOFAC_NAMED_INSTANCE_DEFAULT_KEY)
                .InstancePerLifetimeScope();
            builder.RegisterGenericDecorator(
                typeof(ContentNoCacheQueries<,>), typeof(IContentQueries<,>), fromKey: DependencyConstants.AUTOFAC_NAMED_INSTANCE_DEFAULT_KEY)
                .InstancePerLifetimeScope();
            builder.RegisterGenericDecorator(
                typeof(CommentNoCacheQueries<,,>), typeof(ICommentQueries<,,>), fromKey: DependencyConstants.AUTOFAC_NAMED_INSTANCE_DEFAULT_KEY)
                .InstancePerLifetimeScope();
        }
    }
}
