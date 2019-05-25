using Moq;
using NUnit.Framework;
using Sanatana.Contents.Database.EntityFrameworkCore.Context;
using Sanatana.Contents.Database.EntityFrameworkCore.Queries;
using Sanatana.Contents.Database.EntityFrameworkCoreSpecs.TestTools.Interfaces;
using Sanatana.EntityFrameworkCore;
using SpecsFor;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using Sanatana.Contents.Database.EntityFrameworkCoreSpecs.SampleEntities;
using Sanatana.Contents.Objects.DTOs;
using Sanatana.Contents.Objects;

namespace Sanatana.Contents.Database.EntityFrameworkCoreSpecs
{
    public class SqlContentQueriesSpecs
    {
        [TestFixture]
        public class when_inserting_many_content : SpecsFor<SqlContentQueries<Ticket>>, INeedDatabase
        {
            public ContentsDbContext ContentsDb { get; set; }

            protected override void When()
            {
                List<Ticket> content = Enumerable.Range(0, 5)
                    .Select(x => new Ticket
                    {
                        CategoryId = 5,
                        ShortText = "ShortContent " + x,
                        FullText = "FullContent " + x
                    })
                    .ToList();

                SUT.InsertMany(content).Wait();
            }

            [Test]
            public void then_returns_many_inserted_generic_content_using_ef()
            {
                List<Ticket> content = ContentsDb
                    .Set<Ticket>().Where(x => x.CategoryId == 5)
                    .ToList();

                content.ShouldNotBeEmpty();
                content.Count.ShouldEqual(5);
            }
        }
        
        [TestFixture]
        public class when_inserting_same_publish_time_content : SpecsFor<SqlContentQueries<Ticket>>, INeedDatabase
        {
            private Ticket _insertedContent;
            public ContentsDbContext ContentsDb { get; set; }


            protected override void When()
            {
                _insertedContent = new Ticket
                {
                    CategoryId = 6,
                    ShortText = "ShortContent " + 6,
                    FullText = "FullContent " + 6,
                    Url = "url1",
                    PublishedTimeUtc = DateTime.UtcNow
                };
                ContentInsertResult result = SUT.InsertOne(_insertedContent).Result;
                result.ShouldEqual(ContentInsertResult.Success);
            }

            [Test]
            public void then_returns_not_unique_publish_time_on_duplicate_ef()
            {
                var newContent = new Ticket
                {
                    PublishedTimeUtc = _insertedContent.PublishedTimeUtc
                };
                ContentInsertResult sameUrlResult = SUT.InsertOne(newContent).Result;
                sameUrlResult.ShouldEqual(ContentInsertResult.PublishTimeUtcIsNotUnique);
            }

            [Test]
            public void then_returns_single_inserted_content_by_id_ef()
            {
                Ticket content = ContentsDb
                    .Set<Ticket>()
                    .Where(x => x.ContentId == _insertedContent.ContentId)
                    .SingleOrDefault();

                content.ShouldNotBeNull();
                content.PublishedTimeUtc.ShouldEqual(_insertedContent.PublishedTimeUtc);
                content.Url.ShouldEqual(_insertedContent.Url);
            }
        }

        [TestFixture]
        public class when_inserting_same_url_content : SpecsFor<SqlContentQueries<Ticket>>, INeedDatabase
        {
            public ContentsDbContext ContentsDb { get; set; }

            protected override void When()
            {
                var content = new Ticket
                {
                    CategoryId = 6,
                    ShortText = "ShortContent " + 6,
                    FullText = "FullContent " + 6,
                    Url = "url1"
                };
                ContentInsertResult result = SUT.InsertOne(content).Result;
                result.ShouldEqual(ContentInsertResult.Success);
            }

            [Test]
            public void then_returns_not_unique_url_result_ef()
            {
                var content = new Ticket
                {
                    CategoryId = 6,
                    ShortText = "ShortContent " + 6,
                    FullText = "FullContent " + 6,
                    Url = "url1"
                };

                ContentInsertResult sameUrlResult = SUT.InsertOne(content).Result;
                sameUrlResult.ShouldEqual(ContentInsertResult.UrlIsNotUnique);
            }
        }
        
        [TestFixture]
        public class when_selecting_content_with_automapper_projection : SpecsFor<SqlContentQueries<Ticket>>, INeedDatabase
        {
            public ContentsDbContext ContentsDb { get; set; }

            protected override void Given()
            {
                List<Ticket> content = Enumerable.Range(0, 5)
                    .Select(x => new Ticket
                    {
                        CategoryId = 4,
                        ShortText = "ShortContent " + x,
                        FullText = "FullContent " + x
                    })
                    .ToList();

                ContentsDb.AddRange(content);
                ContentsDb.SaveChanges();
            }

            [Test]
            public void then_returns_subset_of_content_fields()
            {
                List<Ticket> content = SUT.SelectMany(1, 10, DataAmount.DescriptionOnly
                    , true, x => x.CategoryId == 4)
                    .Result;

                content.ShouldNotBeEmpty();
                content.Count.ShouldEqual(5);
                content.ForEach(x =>
                {
                    x.CategoryId.ShouldEqual(4);
                    x.ShortText.ShouldBeNull();
                    x.FullText.ShouldBeNull();
                });
            }
        }
        
        [TestFixture]
        public class when_selecting_latest_from_each_category : SpecsFor<SqlContentQueries<Ticket>>, INeedDatabase
        {
            int _categoriesCount;
            int _categoryItemsCount;
            public ContentsDbContext ContentsDb { get; set; }

            protected override void Given()
            {
                List<Ticket> contentList = new List<Ticket>();
                _categoriesCount = 5;
                _categoryItemsCount = 10;

                for (int cat = 0; cat < _categoriesCount; cat++)
                {
                    List<Ticket> group = Enumerable.Range(0, _categoryItemsCount)
                        .Select(x => new Ticket
                        {
                            AuthorId = 9,
                            CategoryId = cat,
                            ShortText = "ShortContent int category " + cat,
                            FullText = "FullContent " + cat
                        })
                        .ToList();
                    contentList.AddRange(group);
                }

                ContentsDb.AddRange(contentList);
                ContentsDb.SaveChanges();
            }

            [Test]
            public void then_returns_latest_from_each_category()
            {
                int eachCategoriesItems = _categoryItemsCount + 1;
                List<ContentCategoryGroupResult<long, Ticket>> contentList = SUT
                    .SelectLatestFromEachCategory(eachCategoriesItems, DataAmount.DescriptionOnly
                    , x => x.AuthorId == 9)
                    .Result;

                contentList.ShouldNotBeEmpty();
                contentList.Count.ShouldEqual(_categoriesCount);
                contentList.ForEach(x =>
                {
                    x.Contents.Count.ShouldEqual(_categoryItemsCount);
                    x.Contents.ForEach(catContent =>
                    {
                        catContent.ShortText.ShouldBeNull();
                        catContent.FullText.ShouldBeNull();
                    });
                });
            }
        }

    }
}
