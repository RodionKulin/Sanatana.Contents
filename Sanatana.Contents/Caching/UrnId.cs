using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Sanatana.Contents.Caching
{
    /// <summary>
    /// Uniform Resource Name generator for cache from entity type name, id field or custom keys. 
    /// Based on ServiceStack UrnId implementation.
    /// </summary>
    public class UrnId
    {
        //fields
        private const char FieldSeperator = ':';
        private const char FieldPartsSeperator = '/';
        private const string KeyJoinSeperator = ",";
        public string TypeName { get; private set; }
        public string IdFieldValue { get; private set; }
        public string IdFieldName { get; private set; }

        const int HasNoIdFieldName = 3;
        const int HasIdFieldName = 4;


        //init
        private UrnId() { }


        //methods
        public static string Create(string objectTypeName, string idFieldValue)
        {
            if (objectTypeName.Contains(FieldSeperator.ToString()))
                throw new ArgumentException("objectTypeName cannot have the illegal characters: ':'", "objectTypeName");

            if (idFieldValue.Contains(FieldSeperator.ToString()))
                throw new ArgumentException("idFieldValue cannot have the illegal characters: ':'", "idFieldValue");

            return $"urn:{objectTypeName}:{idFieldValue}";
        }

        public static string Create(Type objectType, string idFieldValue)
        {
            if (idFieldValue.Contains(FieldSeperator.ToString()))
                throw new ArgumentException("idFieldValue cannot have the illegal characters: ':'", "idFieldValue");

            return $"urn:{objectType.Name}:{idFieldValue}";
        }

        public static string Create<T>()
        {
            return Create<T>("all");
        }

        public static string Create(Type objectType, string idFieldName, string idFieldValue)
        {
            if (idFieldValue.Contains(FieldSeperator.ToString()))
                throw new ArgumentException("idFieldValue cannot have the illegal characters: ':'", "idFieldValue");

            if (idFieldName.Contains(FieldSeperator.ToString()))
                throw new ArgumentException("idFieldName cannot have the illegal characters: ':'", "idFieldName");

            return $"urn:{objectType.Name}:{idFieldName}:{idFieldValue}";
        }

        public static string Create<T>(string idFieldValue)
        {
            return Create(typeof(T), idFieldValue);
        }

        public static string Create<T>(object idFieldValue)
        {
            return Create(typeof(T), idFieldValue.ToString());
        }

        public static string Create<T>(string idFieldName, string idFieldValue)
        {
            return Create(typeof(T), idFieldName, idFieldValue);
        }



        //create with key-value parts
        public static string CreateWithParts(string objectTypeName, params string[] keyParts)
        {
            if (objectTypeName.Contains(FieldSeperator.ToString()))
                throw new ArgumentException($"objectTypeName cannot have the illegal characters: '{FieldSeperator.ToString()}'", nameof(objectTypeName));

            var sb = new StringBuilder();
            foreach (string keyPart in keyParts)
            {
                if (sb.Length > 0)
                    sb.Append(FieldPartsSeperator);
                sb.Append(keyPart);
            }

            return $"urn:{objectTypeName}:{sb.ToString()}";
        }

        public static string CreateWithParts<T>(params string[] keyParts)
        {
            return CreateWithParts(typeof(T).Name, keyParts);
        }

        public static string CreateWithValues(string objectTypeName, params object[] keyParts)
        {
            string[] keys = new string[keyParts.Length];

            for (int i = 0; i < keyParts.Length; i++)
            {
                if (keyParts[i] == null)
                {
                    keys[i] = string.Empty;
                }
                else if (keyParts[i] is IEnumerable)
                {
                    var list = keyParts[i] as IEnumerable;
                    keys[i] = ListToString(list);
                }
                else
                {
                    keys[i] = keyParts[i].ToString();
                }
            }

            return CreateWithParts(objectTypeName, keys);
        }

        public static string CreateWithValues<T>(params object[] keyParts)
        {
            return CreateWithValues(typeof(T).Name, keyParts);
        }

        private static string ListToString(IEnumerable list)
        {
            IEnumerator enumerator = list.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return String.Empty;
            }

            var builder = new StringBuilder()
                .Append(enumerator.Current);

            while (enumerator.MoveNext())
            {
                builder.Append(KeyJoinSeperator).Append(enumerator.Current);
            }

            return builder.ToString();
        }


        //parse Id field
        public static UrnId Parse(string urnId)
        {
            string[] urnParts = urnId.Split(FieldSeperator);
            if (urnParts.Length == HasNoIdFieldName)
                return new UrnId { TypeName = urnParts[1], IdFieldValue = urnParts[2] };

            if (urnParts.Length == HasIdFieldName)
                return new UrnId { TypeName = urnParts[1], IdFieldName = urnParts[2], IdFieldValue = urnParts[3] };

            throw new ArgumentException("Cannot parse invalid urn: '{0}'", urnId);
        }

        public static string GetStringId(string urn)
        {
            return Parse(urn).IdFieldValue;
        }

        public static Guid GetGuidId(string urn)
        {
            return new Guid(Parse(urn).IdFieldValue);
        }

        public static long GetLongId(string urn)
        {
            return long.Parse(Parse(urn).IdFieldValue);
        }
    }
}
