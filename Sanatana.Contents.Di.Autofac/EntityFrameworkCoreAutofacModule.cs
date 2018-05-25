using Autofac;
using Sanatana.Contents.Caching.DataChangeNotifiers;
using Sanatana.Contents.Database;
using Sanatana.Contents.Database.EntityFrameworkCore;
using Sanatana.Contents.Database.EntityFrameworkCore.Context;
using Sanatana.Contents.Database.EntityFrameworkCore.DataChangeNotifiers;
using Sanatana.Contents.Database.EntityFrameworkCore.Queries;
using Sanatana.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Di.Autofac
{
    public class EntityFrameworkCoreAutofacModule : Module
    {
        //fields
        private SqlConnectionSettings _connectionSettings;


        //init
        public EntityFrameworkCoreAutofacModule(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }


        //methods
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_connectionSettings).AsSelf().SingleInstance();
            builder.RegisterType<ContentsDbContextFactory>().As<IContentsDbContextFactory>().SingleInstance();
            builder.RegisterType<SqlDataChangeNotifierFactory>().As<IDataChangeNotifierFactory>().SingleInstance();

            builder.RegisterGeneric(typeof(SqlContentQueries<>))
                .Named(DependencyConstants.AUTOFAC_NAMED_INSTANCE_DEFAULT_KEY, typeof(IContentQueries<,>))
                .InstancePerDependency();
            builder.RegisterGeneric(typeof(SqlCommentQueries<,>))
                .Named(DependencyConstants.AUTOFAC_NAMED_INSTANCE_DEFAULT_KEY, typeof(ICommentQueries<,,>))
                .InstancePerDependency();
            builder.RegisterGeneric(typeof(SqlCategoryQueries<>))
                .Named(DependencyConstants.AUTOFAC_NAMED_INSTANCE_DEFAULT_KEY, typeof(ICategoryQueries<,>))
                .InstancePerDependency();
            builder.RegisterType<SqlCategoryRolePermissionQueries>()
                .Named<ICategoryRolePermissionQueries<long>>(DependencyConstants.AUTOFAC_NAMED_INSTANCE_DEFAULT_KEY)
                .InstancePerDependency();

        }
    }
}
