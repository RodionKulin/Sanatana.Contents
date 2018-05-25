using Nest;

namespace Sanatana.Contents.Search.ElasticSearch.Connection
{
    public interface IElasticClientFactory
    {
        ElasticClient GetClient();
    }
}