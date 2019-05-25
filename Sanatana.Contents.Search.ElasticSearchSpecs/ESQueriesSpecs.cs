using Sanatana.Contents.Search.ElasticSearch;
using Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Interfaces;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using NUnit.Framework;
using Should;
using SpecsFor.ShouldExtensions;
using System.Threading;
using StructureMap;
using Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Objects;
using System.Reflection;
using Elasticsearch.Net;
using Sanatana.Contents.Search.ElasticSearch.Queries;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Search.ElasticSearch.Objects;
using Sanatana.Contents.Search.ElasticSearch.Objects.Entities;

namespace Sanatana.Contents.Search.ElasticSearchSpecs
{
    public class ESQueriesSpecs
    {
        [TestFixture]
        public class when_adding_multiple_new_content : SpecsFor<ElasticSearchQueries<long>>
            , INeedClient
        {
            private List<long> _insertedIds;
            private SearchResult<object> _findResponse;


            public ElasticClient Client { get; set; }

            protected override void Given()
            {
                List<Content<long>> contentList = Enumerable.Range(10, 10)
                       .Select(x => new Content<long>()
                       {
                           ContentId = x,
                           Title = "Insert many with " + x,
                           FullText = "Generate a sequence of integers and create content from them."
                       })
                       .ToList();
                _insertedIds = contentList
                    .Select(x => x.ContentId)
                    .OrderBy(x => x)
                    .ToList();

                SUT.Insert(contentList.Cast<object>().ToList()).Wait();

                Client.Refresh(new RefreshRequest());
            }

            protected override void When()
            {
                _findResponse = SUT
                    .FindByInput(new SearchParams
                    {
                        Input = "",
                        Page = 1,
                        PageSize = 20,
                        Highlight = false,
                        TypesToSearch = new List<EntitySearchParams>
                        {
                            new EntitySearchParams<ContentIndexed<long>>()
                                .Filter(x => _insertedIds.Contains(x.ContentId))
                        }
                    })
                    .Result;
            }

            [Test]
            public void then_adds_multiple_content_with_the_expected_data()
            {
                _findResponse.Items.ShouldNotBeNull();

                _findResponse.Items.ForEach(x =>
                {
                    x.ShouldBeType<Content<long>>()
                        .FullText.ShouldEqual("Generate a sequence of integers and create content from them.");
                });

                List<long> actualIds = _findResponse.Items
                    .Select(x => ((Content<long>)x).ContentId)
                    .OrderBy(x => x)
                    .ToList();
                actualIds.ShouldLookLike(_insertedIds);
            }

        }

        [TestFixture]
        public class when_updating_many_content : SpecsFor<ElasticSearchQueries<long>>
          , INeedClient
        {
            private List<long> _insertedIds;
            private List<Content<long>> _contentList;

            public ElasticClient Client { get; set; }

            protected override void Given()
            {
                _contentList = Enumerable.Range(20, 10)
                    .Select(x => new Content<long>()
                    {
                        ContentId = x,
                        Title = "Update many with " + x,
                        FullText = "The simplest usage of _update_by_query just performs an update on every document in the index without changing the source. "
                    })
                    .ToList();
                SUT.Insert(_contentList.Cast<object>().ToList()).Wait();
            }

            protected override void When()
            {
                _contentList.ForEach(x =>
                {
                    x.Title = "It’s also possible to do this whole thing on multiple indexes and multiple types at once, just like the search API";
                    x.FullText = "Just as in Update API you can set ctx.op to change the operation that is executed";
                    x.Version++;
                });

                List<object> updateList = _contentList.Cast<object>().ToList();
                SUT.Update(updateList).Wait();
                 Client.Refresh(new RefreshRequest());

                _insertedIds = _contentList
                    .Select(x => x.ContentId)
                    .OrderBy(x => x)
                    .ToList();
            }

            [Test]
            public void then_updates_many_content_with_the_expected_data()
            {
                SearchResult<object> findResponse = SUT
                    .FindByInput(new SearchParams
                    {
                        Page = 1,
                        PageSize = 20,
                        Highlight = false,
                        TypesToSearch = new List<EntitySearchParams>
                        {
                            new EntitySearchParams<ContentIndexed<long>>()
                                .Filter(x => _insertedIds.Contains(x.ContentId))
                        }
                    })
                    .Result;

                findResponse.Items.ShouldNotBeNull();
                findResponse.Items.Count.ShouldEqual(10);
                findResponse.Items.ForEach(x => x.ShouldLookLikePartial(new
                {
                    Title = "It’s also possible to do this whole thing on multiple indexes and multiple types at once, just like the search API",
                    FullContent = "Just as in Update API you can set ctx.op to change the operation that is executed"
                }));

                List<long> actualIds = findResponse.Items
                    .Select(x => ((Content<long>)x).ContentId)
                    .OrderBy(x => x)
                    .ToList();
                actualIds.ShouldLookLike(_insertedIds);
            }
        }

        [TestFixture]
        public class when_deleting_content : SpecsFor<ElasticSearchQueries<long>>
            , INeedClient
        {
            private Content<long> _content;

            public ElasticClient Client { get; set; }

            protected override void Given()
            {
                _content = new Content<long>()
                {
                    ContentId = 6,
                    Title = "Delete content",
                    FullText = "The simplest usage of _delete_by_query just performs a deletion on every document that match a query.",
                    Version = 0
                };
                SUT.Insert(new List<object> { _content }).Wait();
                 Client.Refresh(new RefreshRequest());
            }

            protected override void When()
            {
                var deleteContent = new Content<long>
                {
                    ContentId = _content.ContentId,
                    Version = 1
                };

                SUT.Delete(new List<object> { deleteContent }).Wait();
                 Client.Refresh(new RefreshRequest());
            }

            [Test]
            public void then_deleted_content_is_not_found()
            {
                object document = SUT.FindById(6, typeof(ContentIndexed<long>)).Result;
                document.ShouldBeNull();
            }

        }

        [TestFixture]
        public class when_searching_multiple_types : SpecsFor<ElasticSearchQueries<long>>
            , INeedClient
        {
            private List<long> _contentIds;
            private List<long> _commentIds;
            private SearchResult<object> _findResponse;

            public ElasticClient Client { get; set; }

            protected override void Given()
            {
                List<Content<long>> contentList = Enumerable.Range(30, 2)
                       .Select(x => new Content<long>()
                       {
                           ContentId = x,
                           Title = x + "Allows to highlight content search results on one or more fields.",
                           FullText = x + "The implementation uses either the lucene plain highlight, the fast vector highlighter (fvh) or postings highlighter.",
                           State = 1,
                           PublishedTimeUtc = DateTime.UtcNow.AddDays(-1)
                       })
                       .ToList();
                _contentIds = contentList
                    .Select(x => x.ContentId)
                    .OrderBy(x => x)
                    .ToList();
                SUT.Insert(contentList.Cast<object>().ToList()).Wait();

                List<Comment<long>> commentList = Enumerable.Range(30, 2)
                    .Select(x => new Comment<long>()
                    {
                        ContentId = 1,
                        CommentId = x,
                        Text = "Comments highlight alongside content same index " + x,
                    })
                    .ToList();
                _commentIds = commentList
                    .Select(x => x.CommentId)
                    .OrderBy(x => x)
                    .ToList();
                SUT.Insert(commentList.Cast<object>().ToList()).Wait();

                 Client.Refresh(new RefreshRequest());
            }

            protected override void When()
            {
                _findResponse = SUT
                    .FindByInput(new SearchParams
                    {
                        Input = "highlight",
                        Page = 1,
                        PageSize = 10,
                        Highlight = true,
                        TypesToSearch = new List<EntitySearchParams>
                        {
                            new EntitySearchParams<ContentIndexed<long>>()
                                .Filter(x => x.State != 0)
                                .Filter(x => x.PublishedTimeUtc < DateTime.UtcNow)
                            ,
                            new EntitySearchParams<CommentIndexed<long>>()
                                .Filter(x => x.ContentId == 1)
                        }
                    })
                    .Result;
            }

            [Test]
            public void then_finds_multiple_types_with_expected_ids()
            {
                _findResponse.Items.ShouldNotBeNull();
                _findResponse.Items.Count.ShouldEqual(4);

                List<long> actualCommentIds = _findResponse.Items
                    .Where(x => x.GetType() == typeof(Comment<long>))
                    .Select(x => (Comment<long>)x)
                    .Select(x => x.CommentId)
                    .OrderBy(x => x)
                    .ToList();
                actualCommentIds.ShouldLookLike(_commentIds);

                List<long> actualContentIds = _findResponse.Items
                    .Where(x => x.GetType() == typeof(Content<long>))
                    .Select(x => (Content<long>)x)
                    .Select(x => x.ContentId)
                    .OrderBy(x => x)
                    .ToList();
                actualContentIds.ShouldLookLike(_contentIds);
            }

            [Test]
            public void then_highlights_multiple_types()
            {
                _findResponse.Items
                    .Where(x => x.GetType() == typeof(Content<long>))
                    .Select(x => (Content<long>)x)
                    .ToList()
                    .ForEach(x => x.FullText.ShouldContain("<em>highlight</em>"));

                _findResponse.Items
                    .Where(x => x.GetType() == typeof(Comment<long>))
                    .Select(x => (Comment<long>)x)
                    .ToList()
                    .ForEach(x => x.Text.ShouldContain("<em>highlight</em>"));
            }

        }

        [TestFixture]
        public class when_searching_multiple_types_in_predefined_query : SpecsFor<SampleSearchQueries>
            , INeedClient
        {
            private List<long> _contentIds;
            private List<long> _commentIds;
            private SearchResult<object> _findResponse;

            public ElasticClient Client { get; set; }


            protected override void Given()
            {
                List<Content<long>> contentList = Enumerable.Range(40, 2)
                       .Select(x => new Content<long>()
                       {
                           ContentId = x,
                           Title = x + " Predefined Allows to highlight content search results on one or more fields.",
                           FullText = x + "The implementation uses either the lucene plain highlight, the fast vector highlighter (fvh) or postings highlighter.",
                           State = 1,
                           PublishedTimeUtc = DateTime.UtcNow.AddDays(-1)
                       })
                       .ToList();
                _contentIds = contentList
                    .Select(x => x.ContentId)
                    .OrderBy(x => x)
                    .ToList();
                SUT.Insert(contentList.Cast<object>().ToList()).Wait();

                List<Comment<long>> commentList = Enumerable.Range(40, 2)
                    .Select(x => new Comment<long>()
                    {
                        ContentId = 1,
                        CommentId = x,
                        Text = "Predefined Comments highlight alongside content same index " + x,
                    })
                    .ToList();
                _commentIds = commentList
                    .Select(x => x.CommentId)
                    .OrderBy(x => x)
                    .ToList();
                SUT.Insert(commentList.Cast<object>().ToList()).Wait();
                
                 Client.Refresh(new RefreshRequest());
            }

            protected override void When()
            {
                _findResponse = SUT
                    .FindByInput(new SearchParams
                    {
                        Input = "predefined",
                        Page = 1,
                        PageSize = 10,
                        Highlight = true,
                    })
                    .Result;
            }

            [Test]
            public void then_finds_multiple_types_with_expected_ids_from_predefined_query()
            {
                _findResponse.Items.ShouldNotBeNull();
                _findResponse.Items.Count.ShouldEqual(4);

                List<long> actualCommentIds = _findResponse.Items
                    .Where(x => x.GetType() == typeof(Comment<long>))
                    .Select(x => (Comment<long>)x)
                    .Select(x => x.CommentId)
                    .OrderBy(x => x)
                    .ToList();
                actualCommentIds.ShouldLookLike(_commentIds);

                List<long> actualContentIds = _findResponse.Items
                    .Where(x => x.GetType() == typeof(Content<long>))
                    .Select(x => (Content<long>)x)
                    .Select(x => x.ContentId)
                    .OrderBy(x => x)
                    .ToList();
                actualContentIds.ShouldLookLike(_contentIds);
            }

            [Test]
            public void then_highlights_multiple_types_from_predefined_query()
            {
                _findResponse.Items
                    .Where(x => x.GetType() == typeof(Content<long>))
                    .Select(x => (Content<long>)x)
                    .ToList()
                    .ForEach(x => x.Title.ShouldContain("<em>Predefined</em>"));

                _findResponse.Items
                    .Where(x => x.GetType() == typeof(Comment<long>))
                    .Select(x => (Comment<long>)x)
                    .ToList()
                    .ForEach(x => x.Text.ShouldContain("<em>Predefined</em>"));
            }

        }

        [TestFixture]
        public class when_morphology_searching_content : SpecsFor<ElasticSearchQueries<long>>
            , INeedClient
        {
            private Content<long> _content;

            public ElasticClient Client { get; set; }

            protected override void Given()
            {
                _content = new Content<long>()
                {
                    ContentId = 3,
                    Title = "Looking for a document using morphology",
                    FullText = "The following table shows the compatible versions of Elasticsearch and Morphological Analysis Plugin."
                };
                SUT.Insert(new List<object> { _content }).Wait();
                 Client.Refresh(new RefreshRequest());

            }

            [Test]
            public void then_finds_using_morphology()
            {
                SearchResult<object> findResponse = SUT
                     .FindByInput(new SearchParams
                     {
                         Input = "looked",
                         Page = 1,
                         PageSize = 10,
                         Highlight = false,
                     })
                    .Result;

                findResponse.Items.FirstOrDefault()
                    .ShouldNotBeNull()
                    .ShouldBeType<Content<long>>()
                    .ShouldLookLike(_content);
            }
        }

        [TestFixture]
        public class when_suggesting_content : SpecsFor<ElasticSearchQueries<long>>
            , INeedClient
        {
            private Content<long> _content;

            public ElasticClient Client { get; set; }

            protected override void Given()
            {
                _content = new Content<long>()
                {
                    ContentId = 5,
                    Title = "Search APIs Suggesters.",
                    FullText = "The suggest feature suggests similar looking terms based on a provided text by using a suggester."
                };
                SUT.Insert(new List<object> { _content }).Wait();

                Client.Refresh(new RefreshRequest());
            }

            [Test]
            public void then_suggests_content()
            {
                List<object> findResponse = SUT
                    .Suggest("Search AP", 1, 10)
                    .Result;

                findResponse.FirstOrDefault()
                    .ShouldNotBeNull()
                    .ShouldBeType<Content<long>>()
                    .ShouldLookLikePartial(new
                    {
                        ContentId = _content.ContentId,
                        Title = _content.Title
                    });
            }
        }


    }
}
