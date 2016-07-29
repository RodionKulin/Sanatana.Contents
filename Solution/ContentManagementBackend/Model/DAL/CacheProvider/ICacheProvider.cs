using System;
using System.Collections.Generic;
using Common.Utility;

namespace ContentManagementBackend
{
    public interface ICacheProvider
    {
        void ClearAll();
        object Get(string key);
        void Set(string key, object value, DateTime? expiryUtc = null);
        void Remove(string key);
    }
}