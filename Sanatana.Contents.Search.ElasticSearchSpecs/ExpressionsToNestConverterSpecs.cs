using NUnit.Framework;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using SpecsFor.ShouldExtensions;
using System.Linq.Expressions;
using Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Objects;
using Sanatana.Contents.Search.ElasticSearch;
using Nest;
using Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Comparers;
using Should.Core;
using System.Reflection;
using Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Extensions;
using StructureMap;
using Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Interfaces;
using Sanatana.Contents.Search.ElasticSearch.ExpressionsToNest;

namespace Sanatana.Contents.Search.ElasticSearchSpecs
{
    public class ExpressionsToNestConverterSpecs
    {
        [TestFixture]
        public class when_converting_static_field : SpecsFor<ExpressionsToNestConverter>
            , INeedDependencies
        {
            //guid
            [Test]
            public void then_static_guid_is_used_in_query()
            {
                Expression<Func<SampleEntity, bool>> expression =
                    p => p.GuidNullableProperty == Guid.Empty;

                QueryBase query = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed));
                TermQuery termQuery = (TermQuery)query;
                termQuery.ShouldNotBeNull();

                termQuery.Field.Name.ShouldEqual(nameof(SampleEntity.GuidNullableProperty));
                termQuery.Value.ShouldEqual(Guid.Empty);
            }


            //datetime
            [Test]
            public void then_static_datetime_is_used_in_query()
            {
                Expression<Func<SampleEntity, bool>> expression =
                    p => p.DateProperty == DateTime.Now;

                TermQuery termQuery = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed))
                    .ShouldNotBeNull()
                    .ShouldBeType<TermQuery>();
                termQuery.Field.Name.ShouldEqual(nameof(SampleEntity.DateProperty));
                termQuery.Value
                    .ShouldBeType<DateTime>()
                    .ShouldEqual(DateTime.Now, DatePrecision.Second);
            }

            [Test]
            public void then_datetime_null_field_is_used_in_query()
            {
                Expression<Func<SampleEntity, bool>> expression =
                    p => p.DateProperty == null;

                TermQuery termQuery = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed))
                    .ShouldNotBeNull()
                    .ShouldBeType<TermQuery>();
                termQuery.Field.Name.ShouldEqual(nameof(SampleEntity.DateProperty));
                termQuery.Value.ShouldEqual(null);
            }

            [Test]
            public void then_datetime_static_variable_is_used_in_range_query()
            {
                DateTime lowerThanTime = DateTime.Now;
                Expression<Func<SampleEntity, bool>> expression =
                    p => p.DateProperty < lowerThanTime;

                DateRangeQuery termQuery = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed))
                    .ShouldNotBeNull()
                    .ShouldBeType<DateRangeQuery>();
                termQuery.Field.Name.ShouldEqual(nameof(SampleEntity.DateProperty));
                termQuery.LessThan.ShouldEqual(lowerThanTime, new DateMathEqualityComparer());
            }

            [Test]
            public void then_datetime_variable_is_used_in_range_query()
            {
                DateTime greaterOrEqual = new DateTime(2014, 1, 5, 15, 55, 45, 55);
                Expression<Func<SampleEntity, bool>> expression =
                    p => p.DateProperty >= greaterOrEqual;

                DateRangeQuery termQuery = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed))
                    .ShouldNotBeNull()
                    .ShouldBeType<DateRangeQuery>();
                termQuery.Field.Name.ShouldEqual(nameof(SampleEntity.DateProperty));
                termQuery.GreaterThanOrEqualTo.ShouldEqual(greaterOrEqual, new DateMathEqualityComparer());
            }
            
            [Test]
            public void then_datetime_property_is_used_in_range_query()
            {
                SampleEntity greater = new SampleEntity()
                {
                    DateProperty = DateTime.Now
                };
                Expression<Func<SampleEntity, bool>> expression =
                    p => p.DateProperty > greater.DateProperty;

                DateRangeQuery termQuery = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed))
                    .ShouldNotBeNull()
                    .ShouldBeType<DateRangeQuery>();
                termQuery.Field.Name.ShouldEqual(nameof(SampleEntity.DateProperty));
                termQuery.GreaterThan.ShouldEqual(greater.DateProperty, new DateMathEqualityComparer());
            }


            //numerics
            [Test]
            public void then_int_null_is_used_in_equals_query()
            {
                Expression<Func<SampleEntity, bool>> expression =
                    p => p.IntProperty == null;

                TermQuery termQuery = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed))
                    .ShouldNotBeNull()
                    .ShouldBeType<TermQuery>();
                termQuery.Field.Name.ShouldEqual(nameof(SampleEntity.IntProperty));
                termQuery.Value.ShouldBeNull();
            }

            [Test]
            public void then_int_variable_is_used_in_equals_query()
            {
                int constantInt = 5;
                Expression<Func<SampleEntity, bool>> expression =
                    p => p.IntProperty == constantInt;

                TermQuery termQuery = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed))
                    .ShouldNotBeNull()
                    .ShouldBeType<TermQuery>();
                termQuery.Field.Name.ShouldEqual(nameof(SampleEntity.IntProperty));
                termQuery.Value.ShouldEqual(constantInt);
            }

            [Test]
            public void then_int_property_is_used_in_range_query()
            {
                SampleEntity greater = new SampleEntity()
                {
                     IntNotNullProperty = 55
                };
                Expression<Func<SampleEntity, bool>> expression =
                    p => p.IntNotNullProperty > greater.IntNotNullProperty;

                NumericRangeQuery termQuery = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed))
                    .ShouldNotBeNull()
                    .ShouldBeType<NumericRangeQuery>();
                termQuery.Field.Name.ShouldEqual(nameof(SampleEntity.IntNotNullProperty));
                termQuery.GreaterThan.ShouldEqual(greater.IntNotNullProperty);
            }

            [Test]
            public void then_byte_property_is_used_in_range_query()
            {
                byte greater = 250;
                Expression<Func<SampleEntity, bool>> expression =
                    p => p.IntNotNullProperty >= greater;

                NumericRangeQuery termQuery = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed))
                    .ShouldNotBeNull()
                    .ShouldBeType<NumericRangeQuery>();
                termQuery.Field.Name.ShouldEqual(nameof(SampleEntity.IntNotNullProperty));
                termQuery.GreaterThanOrEqualTo.ShouldEqual(greater);
            }


            //strings
            [Test]
            public void then_string_variable_is_used_in_equals_query()
            {
                string compareMe = "compareMe";
                Expression<Func<SampleEntity, bool>> expression =
                    p => p.StringProperty == compareMe;

                TermQuery termQuery = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed))
                    .ShouldNotBeNull()
                    .ShouldBeType<TermQuery>();
                termQuery.Field.Name.ShouldEqual(nameof(SampleEntity.StringProperty));
                termQuery.Value.ShouldEqual(compareMe);
            }



            //complex properties
            [Test]
            public void then_complex_property_is_used_in_equals_query()
            {
                string compareMe = "compareMe";
                Expression<Func<ParentEntity, bool>> expression =
                    p => p.Embedded.Address == compareMe;

                TermQuery termQuery = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed))
                    .ShouldNotBeNull()
                    .ShouldBeType<TermQuery>();
                string exprectedName = nameof(ParentEntity.Embedded) + "." + nameof(EmbeddedEntity.Address);
                termQuery.Field.Name.ShouldEqual(exprectedName);
                termQuery.Value.ShouldEqual(compareMe);
            }


            //contains
            [Test]
            public void then_datetime_is_used_in_contains_query()
            {
                DateTime datetime = new DateTime(2014, 1, 5, 15, 55, 45, 55);
                List<DateTime?> list = new List<DateTime?>() { datetime, datetime };
                Expression<Func<SampleEntity, bool>> expression =
                    p => list.Contains(p.DateProperty);

                TermsQuery termQuery = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed))
                    .ShouldNotBeNull()
                    .ShouldBeType<TermsQuery>();
                termQuery.Field.Name.ShouldEqual(nameof(SampleEntity.DateProperty));
                termQuery.Terms.ShouldEqual(list.Cast<object>());
            }
            [Test]
            public void then_guid_is_used_in_contains_query()
            {
                List<Guid> list = new List<Guid>()
                {
                    new Guid("{23D0F191-9075-49CB-8EED-A5BAF77E2985}"),
                    new Guid("{54307A63-CE51-4551-85ED-DDCDDAEF229B}")
                };
                Expression<Func<SampleEntity, bool>> expression =
                    p => list.Contains(p.GuidProperty);

                TermsQuery termQuery = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed))
                    .ShouldNotBeNull()
                    .ShouldBeType<TermsQuery>();
                termQuery.Field.Name.ShouldEqual(nameof(SampleEntity.GuidProperty));
                termQuery.Terms.ShouldEqual(list.Cast<object>());
            }


            //and, or
            [Test]
            public void then_multiple_queries_are_combined_using_and_with_or()
            {
                Expression<Func<SampleEntity, bool>> expression =
                    t => t.IntNotNullProperty == 15 || (t.IntNotNullProperty > 0 && t.IntNotNullProperty <= 10);

                //t.IntProperty == 15 || (t.IntProperty > 0 && t.IntProperty <= 10)
                BoolQuery rootQuery = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed))
                    .ShouldNotBeNull()
                    .ShouldBeType<BoolQuery>();

                List<QueryContainer> combinedOrQueries = rootQuery.Should.ToList();
                combinedOrQueries.Count.ShouldEqual(2);

                //t.IntNotNullProperty == 15
                TermQuery equalQuery = combinedOrQueries[0].GetContainedQuery()
                    .ShouldNotBeNull()
                    .ShouldBeType<TermQuery>();
                equalQuery.Field.Name.ShouldEqual(nameof(SampleEntity.IntNotNullProperty));
                equalQuery.Value.ShouldEqual(15);

                //(t.IntNotNullProperty > 0 && t.IntNotNullProperty <= 10)
                BoolQuery rangeDoubleQuery = combinedOrQueries[1].GetContainedQuery()
                    .ShouldNotBeNull()
                    .ShouldBeType<BoolQuery>();

                List<QueryContainer> combinedAndQueries = rangeDoubleQuery.Must.ToList();
                combinedAndQueries.Count.ShouldEqual(2);

                //t.IntNotNullProperty > 0
                NumericRangeQuery greaterZeroQuery = combinedAndQueries[0].GetContainedQuery()
                    .ShouldNotBeNull()
                    .ShouldBeType<NumericRangeQuery>();
                greaterZeroQuery.Field.Name.ShouldEqual(nameof(SampleEntity.IntNotNullProperty));
                greaterZeroQuery.GreaterThan.ShouldEqual(0);

                //t.IntNotNullProperty <= 10
                NumericRangeQuery lowerThanTenQuery = combinedAndQueries[1].GetContainedQuery()
                    .ShouldNotBeNull()
                    .ShouldBeType<NumericRangeQuery>();
                lowerThanTenQuery.Field.Name.ShouldEqual(nameof(SampleEntity.IntNotNullProperty));
                lowerThanTenQuery.LessThanOrEqualTo.ShouldEqual(10);
            }


            //not equals
            [Test]
            public void then_datetime_property_is_used_in_not_equal_query()
            {
                SampleEntity notEquals = new SampleEntity()
                {
                    DateProperty = DateTime.Now
                };
                Expression<Func<SampleEntity, bool>> expression =
                    p => p.DateProperty != notEquals.DateProperty;

                BoolQuery query = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed))
                    .ShouldNotBeNull()
                    .ShouldBeType<BoolQuery>();
                query.MustNot
                    .ShouldNotBeNull()
                    .Count().ShouldEqual(1);

                TermQuery conditionQuery = query.MustNot.First().GetContainedQuery()
                    .ShouldNotBeNull()
                    .ShouldBeType<TermQuery>();

                conditionQuery.Field.Name.ShouldEqual(nameof(SampleEntity.DateProperty));
                conditionQuery.Value.ShouldEqual(notEquals.DateProperty);
            }


            //exceptions
            [Test]
            [ExpectedException(typeof(NotImplementedException))]
            public void throws_exception_on_compare_field_to_field()
            {
                Expression<Func<SampleEntity, bool>> expression =
                    p => p.IntProperty == p.IntProperty;
                QueryBase query = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed));
            }

            [Test]
            [ExpectedException(typeof(NotImplementedException))]
            public void throws_exception_on_compare_value_to_value()
            {
                Expression<Func<SampleEntity, bool>> expression =
                    p => 5 == 6;
                QueryBase query = SUT.ToNestQuery(expression, typeof(SampleEntityIndexed));
            }
        }

    }
}
