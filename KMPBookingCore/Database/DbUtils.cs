using System;
using System.Reflection;
using System.Text;

namespace KMPBookingCore.Database
{
    public static class DbUtils
    {
        public static string NullString = "NULL";
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
            foreach (var property in type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public))
            {
                if (property.GetCustomAttribute<DbPrimaryKeyAttribute>() != null)
                {
                    return property;
                }
            }
            return null;
        }

        public static string GetPrimaryKeyDBFieldName(Type type)
        {
            var primaryKey = GetPrimaryKey(type);
            return GetDbFieldName(primaryKey);
        }

        public static string GetDbFieldName(PropertyInfo property, bool includeBrackets=true)
        {
            var dbfield = property.GetCustomAttribute<DbFieldAttribute>();
            // There may be fields that are db fields and allowed to not have DbField attribute.
            if (dbfield != null)
            {
                var attributeProvidedName = dbfield.FieldName;
                if (!string.IsNullOrEmpty(attributeProvidedName))
                {
                    return attributeProvidedName;
                }
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
            if (property.PropertyType.GetCustomAttribute<DbClassAttribute>() != null && !sb.ToString().EndsWith("ID", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append(" ID");
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

        public static string ToDbString(Type type, object value, PropertyInfo property=null)
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
                if (property != null && (property.GetCustomAttribute<DbDateOnlyAttribute>()?.IsDateOnly??true))
                {
                    return ((DateTime)value).ToDbDateOnly();
                }
                return ((DateTime)value).ToDbDateTime();
            }
            else if (type == typeof(DateTime?))
            {
                if (property != null && (property.GetCustomAttribute<DbDateOnlyAttribute>()?.IsDateOnly ?? true))
                {
                    return ((DateTime?)value).ToDbDateOnly();
                }
                return ((DateTime?)value).ToDbDateTime();
            }
            else if (type.IsClass && type.GetCustomAttribute<DbClassAttribute>() != null)
            {
                var primaryKeyProperty = GetPrimaryKey(type);
                if (primaryKeyProperty != null)
                {
                    if (value != null)
                    {
                        var id = primaryKeyProperty.GetValue(value);
                        if (primaryKeyProperty.PropertyType == typeof(int))
                        {
                            return id.ToString();
                        }
                        else
                        {
                            return "'" + id.ToString() + "'";
                        }
                    }
                    else
                    {
                        if (primaryKeyProperty.PropertyType == typeof(string))
                        {
                            return NullString;
                        }
                        else if (primaryKeyProperty.PropertyType == typeof(int))
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