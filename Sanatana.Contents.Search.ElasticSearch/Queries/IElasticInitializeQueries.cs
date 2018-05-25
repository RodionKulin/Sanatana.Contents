namespace Sanatana.Contents.Search.ElasticSearch.Queries
{
    public interface IElasticInitializeQueries
    {
        void CreateIndex(int numberOfShards, int numberOfReplicas);
        void DropIndex();
    }
}