using Autofac;
using MongoDB.Bson;
using Sanatana.Contents.Caching.DataChangeNotifiers;
using Sanatana.Contents.Database;
using Sanatana.Contents.Database.MongoDb;
using Sanatana.Contents.Database.MongoDb.Context;
using Sanatana.Contents.Database.MongoDb.DataChangeNotifiers;
using Sanatana.Contents.Database.MongoDb.Queries;
using Sanatana.MongoDb;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Di.Autofac
{
    public class MongoDbAutofacModule : Module
    {
        //fields
        private MongoDbConnectionSettings _mongoDbConnection;


        //init
        public MongoDbAutofacModule(MongoDbConnectionSettings mongoDbConnection)
        {
            _mongoDbConnection = mongoDbConnection;
        }


        //methods
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_mongoDbConnection).AsSelf().SingleInstance();
            builder.RegisterType<ContentMongoDbContext>().As<IContentMongoDbContext>().SingleInstance();
            builder.RegisterType<MongoDbDataChangeNotifierFactory>().As<IDataChangeNotifierFactory>().SingleInstance();

            builder.RegisterGeneric(typeof(MongoDbContentQueries<>))
                .Named(DependencyConstants.AUTOFAC_NAMED_INSTANCE_DEFAULT_KEY, typeof(IContentQueries<,>))
                .InstancePerDependency();
            builder.RegisterGeneric(typeof(MongoDbCommentQueries<,>))
                .Named(DependencyConstants.AUTOFAC_NAMED_INSTANCE_DEFAULT_KEY, typeof(ICommentQueries<,,>))
                .InstancePerDependency();
            builder.RegisterGeneric(typeof(MongoDbCategoryQueries<>))
                .Named(DependencyConstants.AUTOFAC_NAMED_INSTANCE_DEFAULT_KEY, typeof(ICategoryQueries<,>))
                .InstancePerDependency();
            builder.RegisterType<MongoDbCategoryRolePermissionQueries>()
                .Named<ICategoryRolePermissionQueries<ObjectId>>(DependencyConstants.AUTOFAC_NAMED_INSTANCE_DEFAULT_KEY)
                .InstancePerDependency();

        }
    }
}
