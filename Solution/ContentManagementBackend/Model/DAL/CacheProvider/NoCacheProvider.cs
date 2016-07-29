using ContentManagementBackend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utility;

namespace ContentManagementBackend
{
    public class NoCacheProvider: ICacheProvider
    {

        //методы
        public void ClearAll()
        {
        }

        public object Get(string key)
        {
            return null;
        }

        public void Set(string key, object value, DateTime? expiryUtc = default(DateTime?))
        {
        }

        public virtual void Remove(string key)
        {
        }
    }
}
