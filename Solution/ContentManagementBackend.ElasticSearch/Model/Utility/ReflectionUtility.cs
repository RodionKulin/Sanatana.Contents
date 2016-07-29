using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.ElasticSearch
{
    public class ReflectionUtility
    {
        public static string GetCamelCaseName<TDocument>(
            Expression<Func<TDocument, object>> selectMemberLambda)
        {
            var member = selectMemberLambda.Body as MemberExpression;

            if (member == null)
            {
                var unaryExpression = selectMemberLambda.Body as UnaryExpression;
                Type t = unaryExpression.Operand.GetType();
                member = unaryExpression.Operand as MemberExpression;
            }

            if (member == null)
            {
                throw new ArgumentException("The parameter selectMemberLambda must be a member accessing labda such as x => x.Id", "selectMemberLambda");
            }

            return ToCamelCase(member.Member.Name);
        }

        public static PropertyInfo GetPropertyInfo<TDocument>(
            Expression<Func<TDocument, object>> selectMemberLambda)
        {
            var member = selectMemberLambda.Body as MemberExpression;

            if (member == null)
            {
                var unaryExpression = selectMemberLambda.Body as UnaryExpression;
                Type t = unaryExpression.Operand.GetType();
                member = unaryExpression.Operand as MemberExpression;
            }

            if (member == null)
            {
                throw new ArgumentException("The parameter selectMemberLambda must be a member accessing labda such as x => x.Id", "selectMemberLambda");
            }
            
            if (!(member.Member is PropertyInfo))
            {
                throw new ArgumentException("The parameter selectMemberLambda must be a public property");
            }

            return member.Member as PropertyInfo;
        }

        public static string ToCamelCase(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            return word.ToLowerInvariant()[0] + word.Substring(1);
        }

        public static List<List<string>> MergeHighliting<TDocument>(
            List<TDocument> documents, List<HighlightFieldDictionary> highlights
            , params Expression<Func<TDocument, object>>[] properties)
        {
            List<PropertyInfo> propertyInfos = properties.Select(p => GetPropertyInfo(p)).ToList();
            List<string> camelCaseNames = propertyInfos.Select(p => ToCamelCase(p.Name)).ToList();
            List<List<string>> fieldNamesHighlighted = new List<List<string>>();

            for (int i = 0; i < documents.Count; i++)
            {
                fieldNamesHighlighted.Add(new List<string>());

                for (int j = 0; j < propertyInfos.Count; j++)
                {
                    PropertyInfo property = propertyInfos[j];
                    string propertyName = camelCaseNames[j];

                    Highlight fieldHighlight = highlights[i].Values.FirstOrDefault(p =>
                        p.Field == propertyName);

                    if (fieldHighlight != null)
                    {
                        string value = fieldHighlight.Highlights.First();
                        property.SetValue(documents[i], value);

                        fieldNamesHighlighted[i].Add(property.Name);
                    }
                }
            }

            return fieldNamesHighlighted;
        }
    }
}
