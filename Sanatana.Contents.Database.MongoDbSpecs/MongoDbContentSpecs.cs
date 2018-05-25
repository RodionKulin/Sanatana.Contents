using NUnit.Framework;
using Sanatana.Contents.Database.MongoDb;
using Sanatana.Contents.Database.MongoDbSpecs.TestTools.Interfaces;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using MongoDB.Bson;
using System.Diagnostics;
using MongoDB.Driver;
using Sanatana.Contents.Database.MongoDb.Context;
using Sanatana.Contents.Database.MongoDb.Queries;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Objects;

namespace Sanatana.Contents.Database.MongoDbSpecs
{
    public class MongoDbContentSpecs
    {
        [TestFixture]
        public class when_selecting_content_with_compare_method : SpecsFor<MongoDbContentQueries<Content<ObjectId>>>
            , INeedMongoDbContext
        {
            private ObjectId _contentId;
            private ObjectId _categoryId;
            public IContentMongoDbContext MongoDbContext { get; set; }

            protected override void Given()
            {
                _contentId = ObjectId.GenerateNewId();
                _categoryId = ObjectId.GenerateNewId();

                var content = new Content<ObjectId>
                {
                    ContentId = _contentId,
                    CategoryId = _categoryId,
                    FullText = "FullText " + _contentId
                };

                MongoDbContext.GetCollection<Content<ObjectId>>().InsertOne(content);
            }

            [Test]
            public void then_returns_contents_using_generic_comparer_method()
            {
                Content<ObjectId> content = UseEqualityComparer(SUT, _contentId, _categoryId);

                content.ShouldNotBeNull();
            }

            public Content<TKey> UseEqualityComparer<TKey>(IContentQueries<TKey, Content<TKey>> queries, TKey contentId, TKey categoryId)
                where TKey : struct
            {
                return queries.SelectOne(false, DataAmount.FullContent
                    , x => EqualityComparer<TKey>.Default.Equals(x.CategoryId, categoryId))
                    .Result;

            }

        }


    }
}
