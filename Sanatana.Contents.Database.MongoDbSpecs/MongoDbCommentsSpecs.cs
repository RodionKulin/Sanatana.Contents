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
using Sanatana.Contents.Database.MongoDb.Queries;
using Sanatana.Contents.Database.MongoDb.Context;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Objects.DTOs;
using Sanatana.Contents.Objects;

namespace Sanatana.Contents.Database.MongoDbSpecs
{
    public class MongoDbCommentsSpecs
    {
        [TestFixture]
        public class when_selecting_comments : SpecsFor<MongoDbCommentQueries<Content<ObjectId>, Comment<ObjectId>>>
            , INeedMongoDbContext
        {
            private ObjectId _categoryId;
            private int _categoryContentCount;
            public IContentMongoDbContext MongoDbContext { get; set; }

            protected override void Given()
            {
                _categoryId = ObjectId.GenerateNewId();

                List<Comment<ObjectId>> comments = Enumerable.Range(0, 10).Select(x => new Comment<ObjectId>
                {
                    CommentId = ObjectId.GenerateNewId(),
                    ContentId = ObjectId.GenerateNewId(),
                    Text = "Text " + x
                }).ToList();

                List<Content<ObjectId>> contents = new List<Content<ObjectId>>();
                for (int i = 0; i < comments.Count; i++)
                {
                    contents.Add(new Content<ObjectId>
                    {
                        ContentId = comments[i].ContentId,
                        CategoryId = i % 2 == 0
                            ? _categoryId
                            : ObjectId.GenerateNewId(),
                        FullText = "FullText " + i,
                        ShortText = "ShortText " + i
                    });
                }

                _categoryContentCount = contents.Count(x => x.CategoryId == _categoryId);
                
                MongoDbContext.GetCollection<Content<ObjectId>>().InsertMany(contents);
                MongoDbContext.GetCollection<Comment<ObjectId>>().InsertMany(comments);
            }
            
            [Test]
            public void then_returns_comments_filtered_by_category()
            {
                List<CommentJoinResult<ObjectId, Comment<ObjectId>, Content<ObjectId>>> comments = 
                    SUT.SelectManyJoinedContent(
                    1, 10, true, DataAmount.ShortContent
                    , x => x.Content.CategoryId == _categoryId)
                    .Result;
                
                comments.Count.ShouldEqual(_categoryContentCount);
            }

        }

    }
}
