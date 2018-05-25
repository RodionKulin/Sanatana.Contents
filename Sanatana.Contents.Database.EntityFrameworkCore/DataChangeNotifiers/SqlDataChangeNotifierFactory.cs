using Sanatana.Contents.Caching.DataChangeNotifiers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Sanatana.Contents.Database.EntityFrameworkCore.DataChangeNotifiers
{
    public class SqlDataChangeNotifierFactory : IDataChangeNotifierFactory
    {

        //methods
        public IDataChangeNotifier Create<T>(Expression<Func<T, bool>> filter = null, string urnId = null)
        {
            return new SqlDataChangeNotifier<T>(filter);
        }
    }
}
