using AutoMapper;

namespace Sanatana.Contents.Search.ElasticSearch.Utilities
{
    public interface IElasticSearchAutoMapperFactory
    {
        IMapper GetMapper();
    }
}