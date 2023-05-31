using System.Data.OleDb;
using KMPBookingCore.Database;
using static KMPBookingPlus.Query;

namespace KMPBookingPlus
{
    public static class Update
    {
        private static void SaveData<EntryType, IdType>(OleDbConnection connection, EntriesWithID<EntryType, IdType> data) where EntryType : DbObject
        {
            var primaryKeyDbField = DbUtils.GetPrimaryKeyDBFieldName(typeof(EntryType));
            foreach (var kvp in data.IdToEntry)
            {
                var id = kvp.Key;
                var obj = kvp.Value;
                string statement;
                var tableName = DbUtils.GetTableName<EntryType>();
                if (obj.ObjDbState == DbObject.DbState.New)
                {
                    statement = AccessUtils.CreateInsert(tableName, obj.GetFieldValuePairs());
                }
                else if (obj.ObjDbState == DbObject.DbState.Dirty)
                {
                    var idStr = DbUtils.ToDbString(typeof(IdType), id);
                    statement = AccessUtils.CreateUpdate(tableName, obj.GetFieldValuePairs(), $"{primaryKeyDbField}={idStr}");
                }
                else
                {
                    continue;
                }
                connection.RunNonQuery(statement);
            }
        }

        public static void SaveClientData(OleDbConnection connection, ClientData clientData)
        {
            SaveData(connection, clientData);
        }

        public static void SaveGPData(OleDbConnection connection, GPData gpData)
        {
            SaveData(connection, gpData);
        }

        public static void SaveEventData(OleDbConnection connection, EventData eventData)
        {
            SaveData(connection, eventData);
        }
    }
}
