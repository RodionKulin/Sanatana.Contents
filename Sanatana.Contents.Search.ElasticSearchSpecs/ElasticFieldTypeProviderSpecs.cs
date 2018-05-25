using Nest;
using NUnit.Framework;
using Sanatana.Contents.Search.ElasticSearch;
using Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Interfaces;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using StructureMap;
using Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Objects;
using Sanatana.Contents.Search.ElasticSearch.ExpressionsToNest.FieldTypes;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;
using Sanatana.Contents.Search.ElasticSearch.Objects.Entities;

namespace Sanatana.Contents.Search.ElasticSearchSpecs
{
    public class ElasticFieldTypeProviderSpecs
    {
        [TestFixture]
        public class when_getting_elastic_field_mappings : SpecsFor<ElasticFieldTypeProvider<long>>
           , INeedClient
        {
            public ElasticClient Client { get; set; }


            [Test]
            public void then_returns_renamed_elastic_field_name()
            {
                var sampleEntitySettings = new SampleEntityElasticSettings();

                string fieldName = SUT.GetElasticFieldName(
                    sampleEntitySettings.IndexType, nameof(SampleEntityIndexed.StringProperty));

                fieldName.ShouldNotBeNull()
                    .ShouldEqual("RenamedField");
            }

            [Test]
            public void then_returns_term_query_type_for_keyword()
            {
                var contentDefaultSettings = new ContentElasticSettings<long>();

                ElasticQueryType queryType = SUT.GetElasticFieldType(
                    contentDefaultSettings.IndexType, nameof(ContentIndexed<long>.CategoryId));

                queryType.ShouldEqual(ElasticQueryType.Term);
            }

            [Test]
            public void then_returns_date_field_type()
            {
                var sampleEntitySettings = new SampleEntityElasticSettings();

                ElasticQueryType queryType = SUT.GetElasticFieldType(
                    sampleEntitySettings.IndexType, nameof(SampleEntityIndexed.DateProperty));

                queryType.ShouldEqual(ElasticQueryType.Date);
            }

            [Test]
            public void then_returns_numeric_query_type()
            {
                var sampleEntitySettings = new SampleEntityElasticSettings();

                ElasticQueryType queryType = SUT.GetElasticFieldType(
                    sampleEntitySettings.IndexType, nameof(SampleEntityIndexed.IntNotNullProperty));

                queryType.ShouldEqual(ElasticQueryType.Numeric);
            }

            [Test]
            [ExpectedException(typeof(ArgumentException))]
            public void then_throws_exception_on_ignored_member_usage()
            {
                var sampleEntitySettings = new SampleEntityElasticSettings();

                ElasticQueryType queryType = SUT.GetElasticFieldType(
                    sampleEntitySettings.IndexType, nameof(SampleEntityIndexed.IntProperty));
            }
        }
    }
}
