using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sanatana.Contents.Caching.CacheProviders;

namespace Sanatana.Contents.Caching.DataChangeNotifiers
{
    public class DataChangeNotifiersRegistry : IDataChangeNotifiersRegistry
    {
        //fields
        protected ReaderWriterLockSlim _subscriptionsLocker;
        protected Dictionary<string, DataChangeSubscription> _subscriptions;
        protected ICacheProvider _cacheProvider;


        //init
        public DataChangeNotifiersRegistry(ICacheProvider cacheProvider)
        {
            _subscriptionsLocker = new ReaderWriterLockSlim();
            _subscriptions = new Dictionary<string, DataChangeSubscription>();
            _cacheProvider = cacheProvider;
        }


        //methods
        public virtual async Task Register(IDataChangeNotifier changeNotifier, string cacheKey)
        {
            //create subscription to ChangeNotifier if not yet exist
            string notifierUrnId = changeNotifier.GetNotifierUrnId();
            bool isFirstSubsciption = false;

            bool subscriptionExists = GetSubscriptionExists(notifierUrnId, cacheKey);
            if (subscriptionExists == false)
            {
                AddSubscription(notifierUrnId, cacheKey, changeNotifier, out isFirstSubsciption);
            }

            //start monitoring
            if (isFirstSubsciption)
            {
                changeNotifier.Start();
            }

            //check new change notifications.
            //new subsciption and cache data is also cleared here cause it have not yet requested fresh data.
            bool hasChanges = GetHasChanges();
            if (hasChanges)
            {
                await InvalidateCache().ConfigureAwait(false);
            }
        }

        protected virtual bool GetSubscriptionExists(string notifierUrnId, string cacheKey)
        {
            try
            {
                _subscriptionsLocker.EnterReadLock();

                if (_subscriptions.ContainsKey(notifierUrnId) == false)
                {
                    return false;
                }

                DataChangeSubscription cacheKeys = _subscriptions[notifierUrnId];
                return cacheKeys.SubscribedCacheKeys.Contains(cacheKey);
            }
            finally
            {
                _subscriptionsLocker.ExitReadLock();
            }
        }

        protected virtual void AddSubscription(string notifierUrnId, string cacheKey
            , IDataChangeNotifier changeNotifier, out bool isFirstSubsciption)
        {
            isFirstSubsciption = false;

            try
            {
                _subscriptionsLocker.EnterWriteLock();

                //create subscription
                if (_subscriptions.ContainsKey(notifierUrnId) == false)
                {
                    _subscriptions.Add(notifierUrnId, new DataChangeSubscription(changeNotifier));
                }

                //check if need to start monitoring
                DataChangeSubscription subscription = _subscriptions[notifierUrnId];
                isFirstSubsciption = subscription.SubscribedCacheKeys.Count == 0;
                
                //add cache key to subscription
                subscription.SubscribedCacheKeys.Add(cacheKey);
            }
            finally
            {
                _subscriptionsLocker.ExitWriteLock();
            }
        }

        protected virtual bool GetHasChanges()
        {
            try
            {
                _subscriptionsLocker.EnterReadLock();
                return _subscriptions.Any(x => x.Value.HasChanges);
            }
            finally
            {
                _subscriptionsLocker.ExitReadLock();
            }
        }

        protected virtual async Task InvalidateCache()
        {
            try
            {
                _subscriptionsLocker.EnterWriteLock();

                foreach (DataChangeSubscription subscription in _subscriptions.Values)
                {
                    if (subscription.HasChanges == false)
                    {
                        continue;
                    }

                    await _cacheProvider.Remove(subscription.SubscribedCacheKeys).ConfigureAwait(false);
                    subscription.SubscribedCacheKeys.Clear();
                    subscription.HasChanges = false;
                }
            }
            finally
            {
                _subscriptionsLocker.ExitWriteLock();
            }
        }

        public virtual void HandleChangeNotification(IDataChangeNotifier changeNotifier)
        {
            string notifierUrnId = changeNotifier.GetNotifierUrnId();

            try
            {
                _subscriptionsLocker.EnterWriteLock();

                if (_subscriptions.ContainsKey(notifierUrnId))
                {
                    _subscriptions[notifierUrnId].HasChanges = true;
                }
            }
            finally
            {
                _subscriptionsLocker.ExitWriteLock();
            }
        }
    }


}
