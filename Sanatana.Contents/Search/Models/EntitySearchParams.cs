using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Sanatana.Contents.Search
{
    public abstract class EntitySearchParams
    {
        public abstract Type IndexType { get; }
        public abstract List<Expression> FilterExpressions { get; }
    }
}
