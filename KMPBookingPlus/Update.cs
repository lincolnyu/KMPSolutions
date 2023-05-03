using System.Data.OleDb;
using KMPBookingCore.Database;
using static KMPBookingPlus.Query;

namespace KMPBookingPlus
{
    public static class Update
    {
        public static void SaveClientData(OleDbConnection connection, ClientData clientData)
        {
            foreach (var kvp in clientData.IdToEntry)
            {
                var id = kvp.Key;
                var client = kvp.Value;
                string statement;
                if (client.ObjDbState == DbObject.DbState.New)
                {
                    statement = AccessUtils.CreateInsert("Client", client.GetFieldValuePairs());
                }
                else if (client.ObjDbState == DbObject.DbState.Dirty)
                {
                    statement = AccessUtils.CreateUpdate("Client", client.GetFieldValuePairs(), $"[Medicare Number]={id}");
                }
                else
                {
                    continue;
                }
                connection.RunNonQuery(statement);
            }
        }

        public static void SaveGPData(OleDbConnection connection, GPData gpData)
        {
            foreach (var kvp in gpData.IdToEntry)
            {
                var id = kvp.Key;
                var gp = kvp.Value;
                string statement;
                if (gp.ObjDbState == DbObject.DbState.New)
                {
                    statement = AccessUtils.CreateInsert("GP", gp.GetFieldValuePairs());
                }
                else if (gp.ObjDbState == DbObject.DbState.Dirty)
                {
                    statement = AccessUtils.CreateUpdate("GP", gp.GetFieldValuePairs(), $"[Provider Number]={id}");
                }
                else
                {
                    continue;
                }
                connection.RunNonQuery(statement);
            }
        }

        public static void SaveEventData(OleDbConnection connection, EventData eventData)
        {
            foreach (var kvp in eventData.IdToEntry)
            {
                var id = kvp.Key;
                var e = kvp.Value;

            }
        }
    }
}
