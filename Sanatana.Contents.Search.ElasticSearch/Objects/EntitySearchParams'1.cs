using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Sanatana.Contents.Search.ElasticSearch.Objects
{
    public class EntitySearchParams<T> : EntitySearchParams
        where T : IElasticSearchable
    {
        //properties
        public override Type IndexType
        {
            get
            {
                return typeof(T);
            }
        }
        public override List<Expression> FilterExpressions { get;  } = new List<Expression>();


        //methods
        /// <summary>
        /// Filter results using expressions. 
        /// ==, !=, <, >, <=, >=, &&, || and Contains method are supported.
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        public EntitySearchParams<T> Filter(Expression<Func<T, bool>> filterExpression)
        {
            FilterExpressions.Add(filterExpression);
            return this;
        }


    }
}
