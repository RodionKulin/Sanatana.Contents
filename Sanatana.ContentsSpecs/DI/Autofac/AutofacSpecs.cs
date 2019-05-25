using Autofac;
using NUnit.Framework;
using Sanatana.Contents.DI.Autofac;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using Should.Core;
using Sanatana.Contents.Database;
using Sanatana.Contents.Files.Queries;
using Sanatana.Contents.Caching.Concrete;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Selectors.Contents;
using Sanatana.Contents.Objects;
using Sanatana.ContentsSpecs.TestTools.Objects;
using MongoDB.Bson;
using Sanatana.Contents.Database.MongoDb.DI.Autofac;
using Sanatana.MongoDb;
using Sanatana.Contents.Pipelines.Comments;
using Sanatana.Contents.Search;
using Sanatana.Contents.Pipelines.Categories;
using Sanatana.Contents.Pipelines.Contents;

namespace Sanatana.Contents.DI.AutofacSpecs
{
    public class AutofacSpecs
    {
        public abstract class AutofacSetupBase : SpecsFor<ContainerBuilder>
        {
            protected ContainerBuilder _builder;

            public AutofacSetupBase()
            {
                _builder = new ContainerBuilder();
                _builder.RegisterModule(new ContentAutofacModule());
                _builder.RegisterModule(new MongoDbAutofacModule(new MongoDbConnectionSettings
                {
                    DatabaseName = "SanatanaContentsSpecs",
                    Host = "localhost",
                    Port = 27017,
                }));
                _builder.RegisterModule(new CachingAutofacModule());
                _builder.RegisterModule(new FilesAutofacModule(new FileStorageSettings
                {
                    BaseDirectory = "temp"
                }));
                _builder.RegisterType<StubUserRoleQueries>().As<IUserRolesQueries<ObjectId>>().SingleInstance();
                _builder.RegisterType<NoSearchQueries<ObjectId>>().As<ISearchQueries<ObjectId>>().SingleInstance();
            }
        }

        [TestFixture]
        public class when_resolving_generic_selector : AutofacSetupBase
        {
            [Test]
            public void then_returns_overridern_selector_instance_from_autofac()
            {
                IContainer container = _builder.Build();
                var selector1 = container.Resolve<IContentSelector<ObjectId, Category<ObjectId>, Content<ObjectId>>>();
                selector1.ShouldNotBeNull();

                var selector2 = container.Resolve<IContentSelector<ObjectId, Category<ObjectId>, Ticket<ObjectId>>>();
                selector2.ShouldNotBeNull();
            }
        }

        [TestFixture]
        public class when_overriding_autofac_concrete_type_cache_queries_registration : AutofacSetupBase
        {
            IContainer _container;

            protected override void Given()
            {
                _builder.RegisterDecorator<ICategoryQueries<ObjectId, TicketCategory>>(
                   (c, inner) => new CategoryNoCacheQueries<ObjectId, TicketCategory>(inner),
                   fromKey: DependencyConstants.AUTOFAC_NAMED_INSTANCE_DEFAULT_KEY);

                _container = _builder.Build();
            }

            [Test]
            public void then_returns_custom_concrete_type_cached_queries_instance()
            {
                var noCacheQueries = _container.Resolve<ICategoryQueries<ObjectId, TicketCategory>>();
                noCacheQueries.ShouldNotBeNull()
                    .ShouldBeType<CategoryNoCacheQueries<ObjectId, TicketCategory>>();
            }

            [Test]
            public void then_still_returns_default_generic_cached_queries_instance_on_other_generic_params()
            {
                var defaultQueries = _container.Resolve<ICategoryQueries<ObjectId, Category<ObjectId>>>();
                defaultQueries.ShouldNotBeNull()
                    .ShouldBeType<CategoryFullyCachedQueries<ObjectId, Category<ObjectId>>>();
            }
        }

        [TestFixture]
        public class when_overriding_default_generic_type_autofac_cache_queries_registration : AutofacSetupBase
        {
            IContainer _container;

            protected override void Given()
            {
                _builder.RegisterGenericDecorator(
                   typeof(CategoryNoCacheQueries<,>), typeof(ICategoryQueries<,>), fromKey: DependencyConstants.AUTOFAC_NAMED_INSTANCE_DEFAULT_KEY)
                   .InstancePerLifetimeScope();

                _container = _builder.Build();
            }

            [Test]
            public void then_returns_overriden_generic_cached_queries()
            {
                var noCacheQueries = _container.Resolve<ICategoryQueries<ObjectId, Category<ObjectId>>>();
                noCacheQueries.ShouldNotBeNull()
                    .ShouldBeType<CategoryNoCacheQueries<ObjectId, Category<ObjectId>>>();
                
                var noCacheQueries2 = _container.Resolve<ICategoryQueries<ObjectId, TicketCategory>>();
                noCacheQueries2.ShouldNotBeNull()
                    .ShouldBeType<CategoryNoCacheQueries<ObjectId, TicketCategory>>();
            }
        }

        [TestFixture]
        public class when_calling_generic_selector : AutofacSetupBase
        {
            [Test]
            public void then_calls_selector_instance_from_autofac()
            {
                IContainer container = _builder.Build();
                var selector1 = container.Resolve<IContentSelector<ObjectId, Category<ObjectId>, Content<ObjectId>>>();
                selector1.ShouldNotBeNull();

                var pageVM = selector1.SelectPage(1, 10
                    , DataAmount.FullContent, true, 0, null, false, x => true).Result;
                pageVM.ShouldNotBeNull();
            }
        }

        [TestFixture]
        public class when_calling_generic_pipeline : AutofacSetupBase
        {
            [Test]
            public void then_pipeline_instance_is_resolved()
            {
                IContainer container = _builder.Build();
                var pipeline = container.Resolve<IDeleteCommentPipeline<ObjectId, Category<ObjectId>, Content<ObjectId>, Comment<ObjectId>>>();
                pipeline.ShouldNotBeNull();
            }
        }

        [TestFixture]
        public class when_calling_all_generic_pipelines : AutofacSetupBase
        {
            [Test]
            public void then_pipeline_instances_are_resolved()
            {
                IContainer container = _builder.Build();

                Type[] resolveTypes = new Type[]
                {
                    typeof(IInsertCategoryPipeline<ObjectId, Category<ObjectId>>),
                    typeof(IUpdateCategoryPipeline<ObjectId, Category<ObjectId>>),
                    typeof(IDeleteCategoryPipeline<ObjectId, Category<ObjectId>>),
                    typeof(IInsertContentPipeline<ObjectId, Category<ObjectId>, Content<ObjectId>>),
                    typeof(IUpdateContentPipeline<ObjectId, Category<ObjectId>, Content<ObjectId>>),
                    typeof(IDeleteContentPipeline<ObjectId, Category<ObjectId>, Content<ObjectId>, Comment<ObjectId>>),
                    typeof(IInsertCommentPipeline<ObjectId, Category<ObjectId>, Content<ObjectId>, Comment<ObjectId>>),
                    typeof(IUpdateCommentPipeline<ObjectId, Category<ObjectId>, Content<ObjectId>, Comment<ObjectId>>),
                    typeof(IDeleteCommentPipeline<ObjectId, Category<ObjectId>, Content<ObjectId>, Comment<ObjectId>>)
                };

                foreach (Type serviceType in resolveTypes)
                {
                    var pipeline = container.Resolve(serviceType);
                    pipeline.ShouldNotBeNull();
                }
            }
        }
    }
}
