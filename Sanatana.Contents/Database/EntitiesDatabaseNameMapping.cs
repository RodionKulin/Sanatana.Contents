using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Database
{
    public class EntitiesDatabaseNameMapping : IEntitiesDatabaseNameMapping
    {
        //fields
        protected Dictionary<Type, string> _entityNames;
        public bool ThrowOnUnknownTypes { get; set; }


        //init
        public EntitiesDatabaseNameMapping()
        {
            _entityNames = new Dictionary<Type, string>();
        }
        public EntitiesDatabaseNameMapping(Dictionary<Type, string> entityNames)
        {
            _entityNames = entityNames;
        }


        //methods
        public virtual string GetEntityName<TEntity>()
        {
            Type entityType = typeof(TEntity);            
            return GetEntityName(entityType);
        }

        public virtual string GetEntityName(Type entityType)
        {
            if (_entityNames.ContainsKey(entityType) == false
                && ThrowOnUnknownTypes)
            {
                throw new NotImplementedException($"Document type {entityType} is not configured. Use {GetType()} to define collection name.");
            }

            if (_entityNames.ContainsKey(entityType) == false)
            {
                string defaultName = GetEntityDefaultName(entityType);
                if (_entityNames.ContainsValue(defaultName))
                {
                    throw new FormatException($"Entity name {defaultName} is already in use. {GetType()} can be used to provide alternative name mapping.");
                }

                _entityNames.Add(entityType, defaultName);
            }

            return _entityNames[entityType];
        }

        protected virtual string GetEntityDefaultName(Type entityType)
        {
            string name = entityType.Name;
            int index = name.IndexOf('`');
            return index == -1 
                ? name 
                : name.Substring(0, index);
        }
    }
}
