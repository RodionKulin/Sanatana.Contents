using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Caching.DataChangeNotifiers
{
    public class DataChangeSubscription
    {
        //properties
        public IDataChangeNotifier ChangeNotifier { get; set; }
        public HashSet<string> SubscribedCacheKeys { get; set; }
        public bool HasChanges { get; set; }


        //init
        public DataChangeSubscription(IDataChangeNotifier changeNotifier)
        {
            ChangeNotifier = changeNotifier;
            SubscribedCacheKeys = new HashSet<string>();
        }
    }
}
