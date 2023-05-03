using System;

namespace KMPBookingCore.Database
{
    public class DBClassAttribute : Attribute
    {
        public DBClassAttribute() { }
        public DBClassAttribute(string tableName)
        {
            TableName = tableName;
        }
        public string TableName { private set; get; }
    }
}
