using Common.Utility;
using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ContentManagementBackend.ElasticSearch
{
    public class ESQueries<TKey> : ISearchQueries<TKey>
        where TKey : struct
    {
        //поля
        protected ICommonLogger _logger;
        protected ElasticClient _client;
        protected ESSettings _settings;
        

        //инициализация
        public ESQueries(ICommonLogger logger)
        {
            _logger = logger;
        }
        public ESQueries(ICommonLogger logger, ESSettings settings)
        {
            _logger = logger;
            _settings = settings;
            _client = CreateElasticClient(settings);
        }
        protected virtual ElasticClient CreateElasticClient(ESSettings settings)
        {
            ConnectionSettings connection = new ConnectionSettings(
                settings.NodeUri, settings.PostIndexName.ToLowerInvariant());
            ElasticClient client = new ElasticClient(connection);
            return client;
        }



        //методы
        public virtual async Task<bool> Insert(ContentBase<TKey> item)
        {
            bool completed = false;

            try
            {
                var indexItem = new ContentIndexed<TKey>(item);
                string id = item.ContentID.ToString();

                IIndexResponse response = await _client.IndexAsync(
                    indexItem, ind => ind.Id(id));

                completed = response.Created;

                if (!completed && response.ConnectionStatus.Success)
                {
                    completed = await Update(item);
                }
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return completed;
        }

        public virtual async Task<bool> Insert(List<ContentBase<TKey>> items)
        {
            bool completed = false;

            try
            {
                List<ContentIndexed<TKey>> indexArticles =
                    items.Select(p => new ContentIndexed<TKey>(p)).ToList();

                int i = 0;

                IBulkResponse response = await _client.BulkAsync(p => p
                    .IndexMany(indexArticles, (bd, item) =>
                    {
                        string id = items[i].ContentID.ToString();
                        i++;
                        return bd.Id(id);
                    })
                );

                completed = response.ConnectionStatus.Success;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return completed;
        }

        public virtual async Task<ContentManagementBackend.SearchResponse<TKey>> Find(
            string input, int page, int pageSize, bool highlight, TKey? category)
        {
            input = Regex.Replace(input, @"[^ \w]", " ");
            input = Regex.Replace(input, @"\s+", " ");

            var result = new ContentManagementBackend.SearchResponse<TKey>()
            {
                Content = new List<ContentBase<TKey>>(),
                HighlightedFieldNames = highlight ? new List<List<string>>() : null
            };

            try
            {
                int skip = ESPaging.ToSkipNumber(page, pageSize);

                ISearchResponse<ContentIndexed<TKey>> listResponse = await _client.SearchAsync<ContentIndexed<TKey>>(s =>
               {
                   s = s.Query(q =>
                   {
                       QueryContainer matchQuery = q.MultiMatch(m => m
                           .OnFieldsWithBoost(b => b
                               .Add(o => o.Title, 3.0)
                               .Add(o => o.FullContent, 2.0)
                           )
                           .Type(TextQueryType.BestFields)
                           .Query(input)
                       );

                       return category == null
                           ? matchQuery
                           : matchQuery && q.Term(ct => ct.CategoryID, category.Value);
                   })
                   .Skip(skip)
                   .Size(pageSize);

                   if (highlight)
                   {
                       s = s.Highlight(h => h
                           .RequireFieldMatch(false)
                           .FragmentSize(_settings.HighlightFragmentSize.Value)
                           .OnFields(
                               f => f.OnField(e => e.Title),
                               f => f.OnField(e => e.FullContent)
                           )
                       );
                   }

                   return s;
               });

                List<ContentIndexed<TKey>> documents = listResponse.Documents.ToList();
                List<HighlightFieldDictionary> highlights = listResponse.Highlights.Values.ToList();
                List<IHit<ContentIndexed<TKey>>> hits = listResponse.Hits.ToList();

                for (int i = 0; i < documents.Count; i++)
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(TKey));
                    TKey idConverted = (TKey)converter.ConvertFromInvariantString(hits[i].Id);

                    ContentIndexed<TKey> indexMatch = documents[i];
                    var item = new ContentBase<TKey>()
                    {
                        ContentID = idConverted,
                        CategoryID = documents[i].CategoryID,
                        Title = documents[i].Title,
                        FullContent = documents[i].FullContent
                    };

                    if (highlight && highlights.Count > 0)
                    {
                        List<string> highlightedFields = new List<string>();

                        string titleKey = ReflectionUtility.ToCamelCase(nameof(indexMatch.Title));
                        Highlight titleHighlight = highlights[i].Values.FirstOrDefault(p =>
                            p.Field == titleKey);
                        if (titleHighlight != null)
                        {
                            highlightedFields.Add(nameof(item.Title));
                            item.Title = titleHighlight.Highlights.First();
                        }

                        string contentKey = ReflectionUtility.ToCamelCase(nameof(indexMatch.FullContent));
                        Highlight contentHighlight = highlights[i].Values.FirstOrDefault(p =>
                            p.Field == contentKey);
                        if (contentHighlight != null)
                        {
                            highlightedFields.Add(nameof(item.FullContent));
                            item.FullContent = contentHighlight.Highlights.First();
                        }

                        result.HighlightedFieldNames.Add(highlightedFields);
                    }

                    result.Content.Add(item);
                }

                result.Total = listResponse.Total;
                result.HasExceptions = !listResponse.ConnectionStatus.Success;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
                result.HasExceptions = true;
            }

            return result;
        }

        public virtual async Task<bool> Update(ContentBase<TKey> item)
        {
            bool completed = false;

            try
            {
                var indexItem = new ContentIndexed<TKey>(item);
                string contentID = item.ContentID.ToString();

                IUpdateResponse response = await _client.UpdateAsync<ContentIndexed<TKey>, object>(u => u
                    .Id(contentID)
                    .Doc(indexItem)
                 );

                completed = response.ConnectionStatus.Success;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return completed;
        }

        public virtual async Task<bool> Delete(TKey id)
        {
            bool completed = false;

            try
            {
                string contentID = id.ToString();

                IDeleteResponse response = await _client.DeleteAsync<ContentIndexed<TKey>>(contentID);

                completed = response.ConnectionStatus.Success;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return completed;
        }

        public virtual async Task<bool> Optimize()
        {
            bool completed = false;

            try
            {
                IShardsOperationResponse response = await _client.OptimizeAsync(op => op
                   .Index(_settings.PostIndexName)
                   .MaxNumSegments(_settings.MaxNumSegments.Value)
                   );
                
                completed = response.ConnectionStatus.Success;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return completed;
        }


    }
}
