using System;
using System.Reflection;
using System.Text;

namespace KMPBookingCore.Database
{
    public static class DbUtils
    {
        public static string NullString = "''";
        public static string NullNumberId = "0";

        public static string GetDbFieldName(string attributeProvidedName, string memberName)
        {
            if (!string.IsNullOrEmpty(attributeProvidedName))
            {
                return attributeProvidedName;
            }
            var sb = new StringBuilder();
            for (var i = 0; i < memberName.Length; ++i)
            {
                var ch = memberName[i];
                if (char.IsUpper(ch) && ((i > 0 && char.IsLower(memberName[i - 1])) || (i < memberName.Length - 1 && char.IsLower(memberName[i + 1]) )))
                {
                    sb.Append(' ');
                }
                sb.Append(ch);
            }
            return sb.ToString();
        }

        public static string ToDbString(Type type, object value)
        {
            if (type == typeof(string))
            {
                return "'" + (string)value + "'";
            }
            else if (type == typeof(TimeSpan))
            {
                return ((TimeSpan)value).TotalMinutes.ToString();
            }
            else if (type == typeof(DateTime))
            {
                return ((DateTime)value).ToDbDateTime();
            }
            else if (type == typeof(DateTime?))
            {
                return ((DateTime?)value).ToDbDateTime();
            }
            else if (value.GetType().GetCustomAttribute<DBClassAttribute>() != null)
            {
                if (value == null)
                {
                    if (type == typeof(string))
                    {
                        return NullString;
                    }
                    else if (type == typeof(int))
                    {
                        return NullNumberId;
                    }
                    else
                    {
                        throw new ArgumentException($"Unexpected ID type to convert to DB: {type}");
                    }
                }
                foreach (var property in value.GetType().GetProperties(BindingFlags.FlattenHierarchy))
                {
                    if (property.GetCustomAttribute<DBPrimaryKeyAttribute>() != null)
                    {
                        var id = property.GetValue(value);
                        return id.ToString();
                    }
                }
                throw new ArgumentException($"Primary key not found in type: {type}");
            }
            else if (value == null)
            {
                return "NULL";
            }
            else
            {
                return value.ToString();
            }
        }
    }
}