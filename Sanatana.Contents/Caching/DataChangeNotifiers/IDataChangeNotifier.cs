using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Caching.DataChangeNotifiers
{
    public interface IDataChangeNotifier
    {
        bool HasChanges { get; }
        string GetNotifierUrnId();
        void Start();
    }
}
