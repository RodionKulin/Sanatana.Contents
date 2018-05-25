using Nest;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using System.Reflection;
using Sanatana.Contents.Search.ElasticSearch.ExpressionsToNest.FieldTypes;

namespace Sanatana.Contents.Search.ElasticSearch.ExpressionsToNest
{
    public class ExpressionsToNestConverter : IExpressionsToNestConverter
    {
        //fields
        protected IElasticFieldTypeProvider _fieldTypeProvider;


        //init
        public ExpressionsToNestConverter(IElasticFieldTypeProvider fieldTypeProvider)
        {
            _fieldTypeProvider = fieldTypeProvider;
        }


        //Expressions to NEST queries
        public virtual QueryBase ToNestQuery(Expression expression, Type indexedType)
        {
            switch (expression.NodeType)
            {
                //lambda
                case ExpressionType.Lambda:
                    var lambda = expression as LambdaExpression;
                    return ToNestQuery(lambda.Body, indexedType);

                //call
                case ExpressionType.Call:
                    var method = expression as MethodCallExpression;
                    return ContainsToQuery(method, indexedType);

                //compare
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThanOrEqual:
                    var binaryCompareExpression = expression as BinaryExpression;
                    return BinaryCompareToQuery(binaryCompareExpression, indexedType);

                //math and logical
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    var binaryCombineExpression = expression as BinaryExpression;
                    return BinarySetToQuery(binaryCombineExpression, indexedType);
            }

            throw new NotImplementedException(
                $"Unknown type of {nameof(ExpressionType)} {expression.GetType().ToString()} and NodeType {expression.NodeType.ToString()}");
        }
        protected virtual ExpressionParam ToQueryParameter(Expression expression, Type indexedType)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    var constant = expression as ConstantExpression;
                    return ExpressionParam.FromValue(constant.Value);

                case ExpressionType.MemberAccess:
                    var memberAccess = expression as MemberExpression;
                    return MemberToQuery(memberAccess, indexedType);

                case ExpressionType.Convert:
                    UnaryExpression unary = expression as UnaryExpression;
                    return ConvertToQuery(unary, indexedType);
            }

            throw new NotImplementedException(
                $"Unknown type of {nameof(ExpressionType)} {expression.GetType().ToString()} and NodeType {expression.NodeType.ToString()}");
        }


        //methods
        protected virtual QueryBase ContainsToQuery(MethodCallExpression expression, Type indexedType)
        {
            //method type
            bool isContains = expression.Method.Name == "Contains";
            if (isContains == false)
                throw new NotImplementedException("Methods other than Contains are not supported.");

            //argument
            Expression containsArgExp = expression.Arguments.FirstOrDefault();
            if (expression.Arguments.Count == 0)
                throw new NotImplementedException("Argument for Contains method is not provided.");

            Type valueType = containsArgExp.Type;
            ExpressionParam field = ToQueryParameter(containsArgExp, indexedType);

            //values list
            var memberObject = expression.Object as MemberExpression;
            if (memberObject == null)
                throw new NotImplementedException("Unknown list type. Could not convert expression to MemberExpression.");

            IEnumerable<object> objectList = MemberToList(memberObject);
            return new TermsQuery
            {
                Field = field.Field,
                Terms = objectList
            };
        }


        //member
        protected virtual ExpressionParam MemberToQuery(MemberExpression expression, Type indexedType)
        {
            ParameterExpression parameterExpression = GetParameterExpression(expression);

            //expression with lambda parameter (left side of expression)
            if (parameterExpression != null)
            {
                string fieldName = ExpressionsReflection.GetMemberName(expression);
                return ExpressionParam.FromField(fieldName, indexedType);
            }
            //object property value
            else if (expression.Member is PropertyInfo)
            {
                PropertyInfo property = (PropertyInfo)expression.Member;
                bool isStatic = property.GetGetMethod().IsStatic;

                if (isStatic)
                {
                    object value = property.GetValue(null);
                    return ExpressionParam.FromValue(value);
                }
                else
                {
                    var member = (MemberExpression)expression.Expression;
                    var constant = (ConstantExpression)member.Expression;
                    var field = ((FieldInfo)member.Member).GetValue(constant.Value);
                    object value = ((PropertyInfo)expression.Member).GetValue(field);
                    return ExpressionParam.FromValue(value);
                }
            }
            //constant value
            else if (expression.Member is FieldInfo)
            {
                var field = (FieldInfo)expression.Member;
                bool isStatic = field.IsStatic;

                if (isStatic)
                {
                    object value = field.GetValue(null);
                    return ExpressionParam.FromValue(value);
                }
                else
                {
                    var constant = (ConstantExpression)expression.Expression;
                    object value = field.GetValue(constant.Value);
                    return ExpressionParam.FromValue(value);
                }
            }
            else
            {
                throw new Exception($"Unknown type of expression {indexedType}");
            }
        }

        protected virtual ParameterExpression GetParameterExpression(MemberExpression expression)
        {
            while (expression != null)
            {
                if (expression.Expression is ParameterExpression)
                {
                    ParameterExpression argumentExpression = expression.Expression as ParameterExpression;
                    return argumentExpression;
                }
                else if (expression.Expression is MemberExpression)
                {
                    expression = expression.Expression as MemberExpression;
                }
                else
                {
                    return null;
                }
            }

            return null;
        }
        
        protected virtual IEnumerable<object> MemberToList(MemberExpression expression)
        {
            Expression innerExpression = expression.Expression;

            bool hasValue = innerExpression.NodeType == ExpressionType.Constant;
            if (hasValue == false)
            {
                throw new NotImplementedException("Unknown enumerable type.");
            }

            var constantExpression = innerExpression as ConstantExpression;
            FieldInfo field = (FieldInfo)expression.Member;
            object fieldValue = field.GetValue(constantExpression.Value);
            return ((IEnumerable)fieldValue).Cast<object>();
        }


        //convert
        protected virtual ExpressionParam ConvertToQuery(UnaryExpression expression, Type indexedType)
        {
            UnaryExpression unary = expression as UnaryExpression;
            ExpressionParam value = ToQueryParameter(unary.Operand, indexedType);
            return value;
        }


        //binary
        protected virtual QueryBase BinaryCompareToQuery(BinaryExpression expression, Type indexedType)
        {
            ExpressionParam fieldParam = ToQueryParameter(expression.Left, indexedType);
            ExpressionParam valueParam = ToQueryParameter(expression.Right, indexedType);

            //change left and right params if field definition is on the right side
            if (fieldParam.IsField == false && valueParam.IsField == true)
            {
                ExpressionParam falseField = fieldParam;
                fieldParam = valueParam;
                valueParam = falseField;
            }
            else if (fieldParam.IsField == false && valueParam.IsField == false
                || fieldParam.IsField == true && valueParam.IsField == true)
            {
                throw new NotImplementedException(
                    $"Invalid expression {expression.ToString()}. Only allowed to compare field to value.");
            }

            return CombineToQuery(expression.NodeType, fieldParam, valueParam);
        }

        protected virtual QueryBase BinarySetToQuery(BinaryExpression expression, Type indexedType)
        {
            QueryBase left = ToNestQuery(expression.Left, indexedType);
            QueryBase right = ToNestQuery(expression.Right, indexedType);
         
            if (expression.NodeType == ExpressionType.And
                || expression.NodeType == ExpressionType.AndAlso)
            {
                return left & right;
            }
            else if (expression.NodeType == ExpressionType.Or
                || expression.NodeType == ExpressionType.OrElse)
            {
                return left | right;
            }

            throw new NotImplementedException(
                $"Unknown nodeType {expression.NodeType}.");
        }

        protected virtual QueryBase CombineToQuery(ExpressionType nodeType
            , ExpressionParam fieldParam, ExpressionParam valueParam)
        {
            if(nodeType == ExpressionType.Equal)
            {
                return new TermQuery
                {
                    Field = fieldParam.Field,
                    Value = valueParam.Value
                };
            }
            else if (nodeType == ExpressionType.NotEqual)
            {
                return new BoolQuery
                {
                    MustNot = new QueryContainer[]
                    {
                        new TermQuery
                        {
                            Field = fieldParam.Field,
                            Value = valueParam.Value
                        }
                    }
                };
            }

            ElasticQueryType elasticQueryType = _fieldTypeProvider.GetElasticFieldType(
                fieldParam.IndexedType, fieldParam.Field.Name);
            
            if (elasticQueryType == ElasticQueryType.Date)
            {
                return CombineToDateTimeQuery(nodeType, fieldParam, valueParam);
            }
            else if (elasticQueryType == ElasticQueryType.Numeric)
            {
                return CombineToNumericQuery(nodeType, fieldParam, valueParam);
            }
            else
            {
                return CombineToTermQuery(nodeType, fieldParam, valueParam);
            }
        }

        protected virtual QueryBase CombineToDateTimeQuery(ExpressionType nodeType
            , ExpressionParam fieldParam, ExpressionParam valueParam)
        {
            switch (nodeType)
            {
                case ExpressionType.LessThan:
                    return new DateRangeQuery
                    {
                        Field = fieldParam.Field,
                        LessThan = (DateTime)valueParam.Value
                    };
                case ExpressionType.LessThanOrEqual:
                    return new DateRangeQuery
                    {
                        Field = fieldParam.Field,
                        LessThanOrEqualTo = (DateTime?)valueParam.Value
                    };
                case ExpressionType.GreaterThan:
                    return new DateRangeQuery
                    {
                        Field = fieldParam.Field,
                        GreaterThan = (DateTime?)valueParam.Value
                    };
                case ExpressionType.GreaterThanOrEqual:
                    return new DateRangeQuery
                    {
                        Field = fieldParam.Field,
                        GreaterThanOrEqualTo = (DateTime?)valueParam.Value
                    };
                default:
                    throw new Exception($"Unknown {nameof(ExpressionType)} {nodeType.ToString()}");
            }
        }

        protected virtual QueryBase CombineToTermQuery(ExpressionType nodeType
            , ExpressionParam fieldParam, ExpressionParam valueParam)
        {
            switch (nodeType)
            {
                case ExpressionType.LessThan:
                    return new TermRangeQuery
                    {
                        Field = fieldParam.Field,
                        LessThan = (string)valueParam.Value
                    };
                case ExpressionType.LessThanOrEqual:
                    return new TermRangeQuery
                    {
                        Field = fieldParam.Field,
                        LessThanOrEqualTo = (string)valueParam.Value
                    };
                case ExpressionType.GreaterThan:
                    return new TermRangeQuery
                    {
                        Field = fieldParam.Field,
                        GreaterThan = (string)valueParam.Value
                    };
                case ExpressionType.GreaterThanOrEqual:
                    return new TermRangeQuery
                    {
                        Field = fieldParam.Field,
                        GreaterThanOrEqualTo = (string)valueParam.Value
                    };
                default:
                    throw new Exception($"Unknown {nameof(ExpressionType)} {nodeType.ToString()}");
            }
        }

        protected virtual QueryBase CombineToNumericQuery(ExpressionType nodeType
            , ExpressionParam fieldParam, ExpressionParam valueParam)
        {
            double? value = valueParam.Value == null
                ? (double?)null
                : Convert.ToDouble(valueParam.Value);

            switch (nodeType)
            {
                case ExpressionType.LessThan:
                    return new NumericRangeQuery
                    {
                        Field = fieldParam.Field,
                        LessThan = value
                    };
                case ExpressionType.LessThanOrEqual:
                    return new NumericRangeQuery
                    {
                        Field = fieldParam.Field,
                        LessThanOrEqualTo = value
                    };
                case ExpressionType.GreaterThan:
                    return new NumericRangeQuery
                    {
                        Field = fieldParam.Field,
                        GreaterThan = value
                    };
                case ExpressionType.GreaterThanOrEqual:
                    return new NumericRangeQuery
                    {
                        Field = fieldParam.Field,
                        GreaterThanOrEqualTo = value
                    };
                default:
                    throw new Exception($"Unknown {nameof(ExpressionType)} {nodeType.ToString()}");
            }
        }
        
    }
}
