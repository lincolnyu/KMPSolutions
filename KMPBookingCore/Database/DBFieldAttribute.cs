using System;

namespace KMPBookingCore.Database
{
    public class DbFieldAttribute : Attribute
    {
        public DbFieldAttribute() { }
        public DbFieldAttribute(string fieldName) 
        {
            FieldName = fieldName;
        }
        public string FieldName { private set; get; }
    }
}
