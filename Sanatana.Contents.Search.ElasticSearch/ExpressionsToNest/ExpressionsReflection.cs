using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sanatana.Contents.Search.ElasticSearch.ExpressionsToNest
{
    public static class ExpressionsReflection
    {
        public static Type GetMemberType(MemberExpression expression)
        {
            MemberInfo member = GetMemberInfo(expression);

            Type type = GetType(member);
            if (Nullable.GetUnderlyingType(type) != null)
            {
                type = Nullable.GetUnderlyingType(type);
            }

            return type;
        }

        private static Type GetType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                default:
                    throw new ArgumentException
                    (
                     "Input MemberInfo must be of type EventInfo, FieldInfo, MethodInfo or PropertyInfo"
                    );
            }
        }

        private static MemberInfo GetMemberInfo(MemberExpression expression)
        {
            MemberInfo latestMember = null;

            while (expression != null)
            {
                latestMember = expression.Member;
                expression = expression.Expression as MemberExpression;
            }

            return latestMember;
        }

        public static string GetMemberName(MemberExpression expression)
        {
            var stack = new List<string>();

            while (expression != null)
            {
                stack.Add(expression.Member.Name);
                expression = expression.Expression as MemberExpression;
            }

            stack.Reverse();
            return ConcatenateEsFieldName(stack);
        }

        private static string ConcatenateEsFieldName(List<string> hierarchyPropertyNames)
        {
            return string.Join(".", hierarchyPropertyNames);
        }

    }
}
