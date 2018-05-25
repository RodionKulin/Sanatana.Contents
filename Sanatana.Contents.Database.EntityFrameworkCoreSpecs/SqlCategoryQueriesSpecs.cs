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
using Sanatana.Contents.Objects.Entities;

namespace Sanatana.Contents.Database.EntityFrameworkCoreSpecs
{
    public class SqlCategoryQueriesSpecs
    {

        [TestFixture]
        public class when_inserting_category_ef : SpecsFor<SqlCategoryQueries<Category<long>>>, INeedDatabase
        {
            public ContentsDbContext ContentsDb { get; set; }

            protected override void Given()
            {
                List<Category<long>> categories = Enumerable.Range(0, 5)
                    .Select(x => new Category<long>
                    {
                        Name = "InsertManyCategory " + x
                    })
                    .ToList();

                SUT.InsertMany(categories).Wait();
            }

            [Test]
            public void then_inserts_generic_category_using_ef()
            {
                List<Category<long>> categories = SUT
                    .SelectMany(x => x.Name.StartsWith("InsertManyCategory"))
                    .Result;

                categories.ShouldNotBeEmpty();
                categories.Count.ShouldEqual(5);
            }
        }
    }

}
