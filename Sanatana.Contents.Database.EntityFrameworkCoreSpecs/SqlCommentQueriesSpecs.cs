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
using Sanatana.Contents.Database.EntityFrameworkCoreSpecs.SampleEntities;
using System.Diagnostics;
using Sanatana.Contents.Objects.DTOs;
using Sanatana.Contents.Objects;

namespace Sanatana.Contents.Database.EntityFrameworkCoreSpecs
{
    public class SqlCommentQueriesSpecs
    {
        [TestFixture]
        public class when_selecting_derived_comment_joined_content_ef
            : SpecsFor<SqlCommentQueries<Ticket, TicketComment>>
            , INeedDatabase
        {
            public ContentsDbContext ContentsDb { get; set; }

            protected override void Given()
            {
                List<Ticket> contents = Enumerable.Range(0, 5)
                    .Select(x => new Ticket
                    {
                        AuthorId = 8,
                        FullText = "FullText " + x,
                        ShortText = "ShortText " + x,
                        UserAssignedTo = 8,
                        IssueId = 88
                    })
                    .ToList();
                SqlContentQueries<Ticket> contentQueries = MockContainer
                    .GetInstance<SqlContentQueries<Ticket>>();
                contentQueries.InsertMany(contents).Wait();

                List<TicketComment> comments = contents
                    .Select(x => new TicketComment
                    {
                        ContentId = x.ContentId,
                        AuthorId = 8,
                        CommitId = Guid.NewGuid()
                    })
                    .ToList();
                SUT.InsertMany(comments).Wait();
            }

            [Test]
            public void then_returns_derived_comments_joined_with_content_using_ef()
            {
                List<CommentJoinResult<long, TicketComment, Ticket>> comments = SUT.SelectManyJoinedContent(
                    1, 10, false, DataAmount.ShortContent, 
                    x => x.Content.AuthorId == 8)
                    .Result;

                comments.ShouldNotBeEmpty();
                comments.Count.ShouldEqual(5);
                comments.ForEach(x =>
                {
                    x.Comment.AuthorId.ShouldEqual(8);
                    x.Content.AuthorId.ShouldEqual(8);
                    x.Content.FullText.ShouldBeNull();
                    x.Content.ShortText.ShouldNotBeNull();
                });
            }
        }
    }

}
