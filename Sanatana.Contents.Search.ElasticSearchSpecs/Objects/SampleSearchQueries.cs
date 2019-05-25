using Sanatana.Contents.Search.ElasticSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;
using Sanatana.Contents.Search.ElasticSearch.Queries;
using Sanatana.Contents.Search.ElasticSearch.Utilities;
using Sanatana.Contents.Search.ElasticSearch.ExpressionsToNest;
using Sanatana.Contents.Search.ElasticSearch.Connection;
using Sanatana.Contents.Search.ElasticSearch.Objects.Entities;

namespace Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Objects
{
    public class SampleSearchQueries : ElasticSearchQueries<long>
    {

        //init
        public SampleSearchQueries(ElasticSettings<long> settings, IElasticClientFactory elasticClientFactory
            , ISearchUtilities searchUtilities, IElasticSearchAutoMapperFactory searchMapperFactory
            , IHighlightMerger highlightMerger, IExpressionsToNestConverter expressionsToNestConverter)
            : base(settings, elasticClientFactory, searchUtilities, searchMapperFactory
                  , highlightMerger, expressionsToNestConverter)
        {
        }


        //methods
        protected override QueryContainer CreateQuery(
            string input, List<EntitySearchParams> typesToSearch)
        {
            QueryBase query = new MatchQuery()
            {
                Field = nameof(IElasticSearchable.AllText),
                Query = input
            };

            query = query && +new BoolQuery
            {
                Filter = new[]
                {
                    new QueryContainer(new BoolQuery
                    {
                        MinimumShouldMatch = 1,
                        Should = new QueryContainer[]
                        {
                            new BoolQuery
                            {
                                Must = new QueryContainer[]
                                {
                                    new TypeQuery
                                    {
                                        Value = TypeName.Create(typeof(ContentIndexed<long>))
                                    },
                                    new TermQuery
                                    {
                                        Field = nameof(ContentIndexed<long>.State),
                                        Value = 0
                                    },
                                    new DateRangeQuery
                                    {
                                        Field = nameof(ContentIndexed<long>.PublishedTimeUtc),
                                        LessThan = DateTime.UtcNow
                                    }
                                }
                            },
                            new BoolQuery
                            {
                                Must = new QueryContainer[]
                                {
                                    new TypeQuery
                                    {
                                        Value = TypeName.Create(typeof(CommentIndexed<long>))
                                    },
                                    new TermQuery
                                    {
                                        Field = nameof(CommentIndexed<long>.ContentId),
                                        Value = 1
                                    }
                                }
                            }
                        }
                    })
                }
            };

            return query;
        }
    }
}
