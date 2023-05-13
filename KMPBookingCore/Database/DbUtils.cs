using System;
using System.Reflection;
using System.Text;

namespace KMPBookingCore.Database
{
    public static class DbUtils
    {
        public static string NullString = "''";
        public static string NullNumberId = "0";

        public static string GetTableName<T>() where T : DbObject
        {
            return GetTableName(typeof(T));
        }

        private static string GetTableName(Type type)
        {
            var dbclass = type.GetCustomAttribute<DbClassAttribute>();
            if (!string.IsNullOrEmpty(dbclass.TableName))
            {
                return dbclass.TableName;
            }
            return type.Name;
        }

        public static PropertyInfo GetPrimaryKey(Type type)
        {
            foreach (var property in type.GetProperties(BindingFlags.FlattenHierarchy))
            {
                if (property.GetCustomAttribute<DbPrimaryKeyAttribute>() != null)
                {
                    return property;
                }
            }
            return null; ;
        }

        public static string GetPrimaryKeyDBFieldName(Type type)
        {
            var primaryKey = GetPrimaryKey(type);
            if (primaryKey == null)
            {
                return GetDbFieldName(primaryKey);
            }
            return null;
        }

        public static string GetDbFieldName(PropertyInfo property, bool includeBrackets=true)
        {
            var dbfield = property.GetCustomAttribute<DbFieldAttribute>();
            var attributeProvidedName = dbfield.FieldName;
            if (!string.IsNullOrEmpty(attributeProvidedName))
            {
                return attributeProvidedName;
            }
            var memberName = property.Name;
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
            var res = sb.ToString().Trim();
            if (includeBrackets && res.Contains(" "))
            {
                res = res.TrimStart('[');
                res = res.TrimEnd(']');
                res = "[" + res + "]";
            }
            return res;
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
            else if (type.GetCustomAttribute<DbClassAttribute>() != null)
            {
                var primaryKeyProperty = GetPrimaryKey(type);
                if (primaryKeyProperty != null)
                {
                    if (value != null)
                    {
                        var id = primaryKeyProperty.GetValue(value);
                        return id.ToString();
                    }
                    else
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