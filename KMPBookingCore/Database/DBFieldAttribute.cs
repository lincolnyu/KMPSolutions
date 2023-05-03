using System;

namespace KMPBookingCore.Database
{
    public class DBFieldAttribute : Attribute
    {
        public DBFieldAttribute() { }
        public DBFieldAttribute(string fieldName) 
        {
            FieldName = fieldName;
        }
        public string FieldName { private set; get; }
    }
}
