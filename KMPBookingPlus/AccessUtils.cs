using System;
using System.Data;
using System.Data.OleDb;

namespace KMPBookingPlus
{
    public static class AccessUtils
    {
        public static string TryGetString(this OleDbDataReader r, int col)
        {
            try
            {
                return r.GetString(col);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static DateTime? TryGetDateTime(this OleDbDataReader r, int col)
        {
            try
            {
                return r.GetDateTime(col);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static OleDbDataReader RunReaderQuery(this OleDbConnection conn, string query, bool closeConnectionOnComplete = true)
        {
            using (var cmd = new OleDbCommand(query, conn))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                var cb = closeConnectionOnComplete ?
                    CommandBehavior.CloseConnection
                    : CommandBehavior.Default;
                var reader = cmd.ExecuteReader(cb);
                return reader;
            }
        }

        public static object RunScalarQuery(this OleDbConnection conn, string query, bool closeConnectionOnComplete = true)
        {
            using (var cmd = new OleDbCommand(query, conn))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                var res = cmd.ExecuteScalar();
                if (closeConnectionOnComplete)
                {
                    conn.Close();
                }
                return res;
            }
        }

        public static void RunNonQuery(this OleDbConnection conn, string cmdstr, bool closeConnectionOnComplete)
        {
            using (var cmd = new OleDbCommand(cmdstr, conn))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.ExecuteNonQuery();
                if (closeConnectionOnComplete)
                {
                    conn.Close();
                }
            }
        }
    }
}
