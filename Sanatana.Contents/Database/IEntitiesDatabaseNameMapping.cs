using System;

namespace Sanatana.Contents.Database
{
    public interface IEntitiesDatabaseNameMapping
    {
        string GetEntityName(Type entityType);
        string GetEntityName<TEntity>();
    }
}