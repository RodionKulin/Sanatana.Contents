using Nest;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sanatana.Contents.Search.ElasticSearch.ExpressionsToNest
{
    public class ExpressionParam
    {
        //properties
        public Type IndexedType { get; set; }
        public Field Field { get; set; }
        public object Value { get; set; }


        //dependent properties
        public bool IsField
        {
            get
            {
                return Field != null;
            }
        }


        //init
        public static ExpressionParam FromField(string fieldName, Type indexedType)
        {
            return new ExpressionParam
            {
                Field = new Field(fieldName),
                IndexedType = indexedType
            };
        }
        public static ExpressionParam FromValue(object value)
        {
            return new ExpressionParam
            {
                Value = value
            };
        }
    }
}
