using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Sanatana.Contents.Caching.DataChangeNotifiers
{ 
    public interface IDataChangeNotifierFactory
    {
        //methods
        IDataChangeNotifier Create<T>(Expression<Func<T, bool>> filter = null, string urnId = null);
    }
}
