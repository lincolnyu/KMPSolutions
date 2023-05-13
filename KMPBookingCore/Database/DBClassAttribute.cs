using System;

namespace KMPBookingCore.Database
{
    public class DbClassAttribute : Attribute
    {
        public DbClassAttribute() { }
        public DbClassAttribute(string tableName)
        {
            TableName = tableName;
        }
        public string TableName { private set; get; }
    }
}
