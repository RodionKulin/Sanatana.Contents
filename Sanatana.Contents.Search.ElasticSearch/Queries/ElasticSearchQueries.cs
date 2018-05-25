using AutoMapper;
using Nest;
using Sanatana.Contents.Search.ElasticSearch.Connection;
using Sanatana.Contents.Search.ElasticSearch.ExpressionsToNest;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;
using Sanatana.Contents.Search.ElasticSearch.Utilities;
using Sanatana.Contents.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sanatana.Contents.Search.ElasticSearch.Queries
{
    public class ElasticSearchQueries<TKey> : ISearchQueries<TKey>
        where TKey : struct
    {
        //fields
        protected ElasticClient _client;
        protected ElasticSettings<TKey> _settings;
        protected ISearchUtilities _utilities;
        protected IMapper _mapper;
        protected IHighlightMerger _highlightMerger;
        protected IExpressionsToNestConverter _expressionsToNestConverter;


        //init
        public ElasticSearchQueries(ElasticSettings<TKey> settings, IElasticClientFactory elasticClientFactory
            , ISearchUtilities searchUtilities, IElasticSearchAutoMapperFactory searchMapperFactory
            , IHighlightMerger highlightMerger, IExpressionsToNestConverter expressionsToNestConverter)
        {
            _settings = settings;
            _client = elasticClientFactory.GetClient();
            _mapper = searchMapperFactory.GetMapper();
            _utilities = searchUtilities;
            _expressionsToNestConverter = expressionsToNestConverter;
            _highlightMerger = highlightMerger;
        }


        //create
        public virtual async Task Insert(List<object> items)
        {
            List<IElasticSearchable> indexItems = _utilities.MapEntitiesToIndexItems(items);

            IBulkResponse response = await _client.BulkAsync(bulk =>
            {
                foreach (IElasticSearchable item in indexItems)
                {
                    Type indexedItemType = item.GetType();
                    ElasticEntitySettings entitySettings = _utilities.FindSettings(indexedItemType);

                    bulk.Index<dynamic>(i => i
                        .Document(item)
                        .Type(entitySettings.ElasticTypeName)
                        .Version(item.Version)
                        .VersionType(Elasticsearch.Net.VersionType.External)
                    );
                }

                return bulk;
            }).ConfigureAwait(false);
        }


        //select
        public virtual async Task<List<object>> Suggest(string input, int page, int pageSize)
        {
            input = _utilities.CleanInput(input);
            int skip = _utilities.ToSkipNumber(page, pageSize);

            ISearchResponse<dynamic> result = await _client
                .SearchAsync<dynamic>(search => search
                    .AllTypes()
                    .Suggest(suggest => suggest
                        .Completion("all-suggestions", completion => completion
                            .Prefix(input)
                            .Field(nameof(IElasticSearchable.Suggest))
                        )
                    )
                    .Skip(skip)
                    .Size(pageSize)
            ).ConfigureAwait(false);

            IEnumerable<dynamic> suggestions = result.Suggest["all-suggestions"]
                .First()
                .Options
                .Select(x => x.Source);

            List<IElasticSearchable> indexItems = _utilities.MapJsonToIndexItems(suggestions);
            List<object> entities = _utilities.MapIndexItemsToEntities(indexItems);

            return entities;
        }

        public virtual async Task<object> FindById(TKey id, Type indexType)
        {
            ElasticEntitySettings entitySettings = _utilities.FindSettings(indexType);

            Id idParam = new Id(id.ToString());
            var request = new GetRequest(_settings.DefaultIndexName, entitySettings.ElasticTypeName, idParam);
            IGetResponse<dynamic> response =
                await _client.GetAsync<dynamic>(request, new CancellationToken())
                .ConfigureAwait(false);

            if (response.Source == null)
            {
                return null;
            }

            List<IElasticSearchable> documents = _utilities
                .MapJsonToIndexItems(new List<dynamic> { response.Source });
            return documents.FirstOrDefault();
        }

        public virtual async Task<SearchResult<object>> FindByInput(SearchParams parameters)
        {
            string input = _utilities.CleanInput(parameters.Input);
            var request = new SearchRequest
            {
                Query = CreateQuery(input, parameters.TypesToSearch),
                From = _utilities.ToSkipNumber(parameters.Page, parameters.PageSize),
                Size = parameters.PageSize
            };

            if (parameters.Highlight)
            {
                request.Highlight = new Highlight
                {
                    FragmentSize = parameters.HighlightFragmentSize.Value,
                    NumberOfFragments = 1,
                    PreTags = new[] { parameters.HighlightPreTags },
                    PostTags = new[] { parameters.HighlightPostTags },
                    Fields = new FluentDictionary<Field, IHighlightField>()
                        .Add("*", new HighlightField()
                        {
                            RequireFieldMatch = false
                        })
                };
            }

            ISearchResponse<dynamic> searchResponse = await _client
                .SearchAsync<dynamic>(request)
                .ConfigureAwait(false);

            List<IElasticSearchable> indexItems = _utilities.MapJsonToIndexItems(searchResponse.Documents);
            if (parameters.Highlight)
            {
                indexItems = _highlightMerger.Merge(indexItems, searchResponse.Hits.ToList());
            }
            return new SearchResult<object>
            {
                Total = searchResponse.Total,
                Items = _utilities.MapIndexItemsToEntities(indexItems)
            };
        }

        protected virtual QueryContainer CreateQuery(string input, List<EntitySearchParams> typesToSearch)
        {
            QueryBase query = new MatchQuery()
            {
                Field = nameof(IElasticSearchable.AllText),
                Query = input
            };

            if (typesToSearch != null && typesToSearch.Count > 0)
            {
                List<QueryContainer> typeFilterQueries = typesToSearch
                    .Select(x => CreateTypeQuery(x))
                    .ToList();
                query = query && +new BoolQuery
                {
                    Filter = new[]
                    {
                        new QueryContainer(new BoolQuery
                        {
                            MinimumShouldMatch = 1,
                            Should = typeFilterQueries
                        })
                    }
                };
            }

            return new QueryContainer(query);
        }

        protected virtual QueryContainer CreateTypeQuery(EntitySearchParams type)
        {
            List<QueryContainer> filterQueries = new List<QueryContainer>();

            filterQueries.Add(new TypeQuery
            {
                Value = TypeName.Create(type.IndexType)
            });

            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            foreach (Expression condition in type.FilterExpressions)
            {
                Expression visitedCondition = visitor.Visit(condition);
                QueryBase filterQuery = _expressionsToNestConverter.ToNestQuery(visitedCondition, type.IndexType);
                filterQueries.Add(new QueryContainer(filterQuery));
            }

            BoolQuery typeQuery = new BoolQuery();
            typeQuery.Must = filterQueries;
            return new QueryContainer(typeQuery);
        }


        //update
        public virtual async Task Update(List<object> items)
        {
            List<IElasticSearchable> indexItems = _utilities.MapEntitiesToIndexItems(items);

            IBulkResponse response = await _client.BulkAsync(bulk =>
            {
                foreach (IElasticSearchable item in indexItems)
                {
                    Type indexedItemType = item.GetType();
                    ElasticEntitySettings entitySettings = _utilities.FindSettings(indexedItemType);

                    bulk.Index<dynamic>(i => i
                        .Document(item)
                        .Type(entitySettings.ElasticTypeName)
                        .Version(item.Version)
                        .VersionType(Elasticsearch.Net.VersionType.External)
                    );
                }

                return bulk;
            }).ConfigureAwait(false);
        }


        //delete
        public virtual async Task Delete(List<object> itemsWithIds)
        {
            List<IElasticSearchable> indexItems = _utilities.MapEntitiesToIndexItems(itemsWithIds);

            IBulkResponse response = await _client.BulkAsync(bulk =>
            {
                foreach (IElasticSearchable item in indexItems)
                {
                    Type indexedItemType = item.GetType();
                    ElasticEntitySettings entitySettings = _utilities.FindSettings(indexedItemType);

                    bulk.Delete<dynamic>(i => i
                        .Document(item)
                        .Type(entitySettings.ElasticTypeName)
                        .Version(item.Version)
                        .VersionType(Elasticsearch.Net.VersionType.External)
                    );
                }

                return bulk;
            }).ConfigureAwait(false);
        }
    }
}
