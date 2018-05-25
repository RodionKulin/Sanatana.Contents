using Sanatana.Contents.Caching;
using Sanatana.Contents.Caching.DataChangeNotifiers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Sanatana.Contents.Database.MongoDb.DataChangeNotifiers
{
    public class MongoDbDataChangeNotifier<T> : IDataChangeNotifier
    {
        //fields
        protected Expression<Func<T, bool>> _filter;
        protected string _urnId;


        //properties
        public bool HasChanges { get; }


        //init
        public MongoDbDataChangeNotifier(Expression<Func<T, bool>> filter = null, string urnId = null)
        {
            _filter = filter;
            _urnId = urnId;
        }


        //methods
        public string GetNotifierUrnId()
        {
            if(_urnId == null)
            {
                if (_filter == null)
                {
                    _urnId = UrnId.Create<T>();
                }
                else
                {
                    string condition = _filter.Body.ToString();
                    _urnId = UrnId.CreateWithParts<T>(condition);
                }
            }

            return _urnId;

            
        }

        public void Start()
        {

        }
    }
}
