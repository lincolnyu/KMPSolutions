using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace KMPBookingCore.Database
{
    public abstract class DbObject : INotifyPropertyChanged
    {
        public enum DbState
        {
            Synced,
            New,
            Dirty,
        }

        public DbState ObjDbState { get; protected set; }

        protected DbObject()
        {
            if (LoadingFromDb)
            {
                ObjDbState = DbState.Synced;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static bool LoadingFromDb { get; private set; }
        private static Stack<bool> _loadingFromDbStack = new Stack<bool>();

        public static void PushLoadingFromDb(bool value)
        {
            _loadingFromDbStack.Push(LoadingFromDb);
            LoadingFromDb = value;
        }

        public static void PopLoadingFromDb()
        {
            LoadingFromDb = _loadingFromDbStack.Pop();
        }

        protected void RaiseEventChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            if (!LoadingFromDb)
            {
                if (ObjDbState == DbState.Synced)
                {
                    ObjDbState = DbState.Dirty;
                }
            }
        }

        public virtual IEnumerable<(string, string)> GetFieldValuePairs()
        {
            var properties = GetType().GetProperties();
            foreach (var property in properties) 
            {
                var dbfield = property.GetCustomAttribute<DbFieldAttribute>();
                if (dbfield != null)
                {
                    var fieldName = DbUtils.GetDbFieldName(property);
                    if (property.PropertyType.GetCustomAttribute<DbClassAttribute>() != null && fieldName.EndsWith("ID", StringComparison.OrdinalIgnoreCase))
                    {
                        fieldName += " ID";
                    }

                    var fieldValue = FieldToDbString(fieldName, property.PropertyType, property.GetValue(this));
                    yield return (fieldName, fieldValue);
                }
            }
        }

        protected virtual string FieldToDbString(string propertyName, Type type, object value)
        {
            return DbUtils.ToDbString(type, value);
        }
    }
}
