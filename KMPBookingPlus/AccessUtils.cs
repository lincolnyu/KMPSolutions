using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Text;
using KMPBookingCore;

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

        public static string CreateInsert(string tableName, IList<(string, string)> fieldValuePairs)
        {
            var sbFields = new StringBuilder();
            var sbValues = new StringBuilder();
            foreach (var fvp in fieldValuePairs)
            {
                var fn = fvp.Item1;
                var fieldName = (fn.Contains(" ") && !fn.StartsWith("[")) ?
                    '[' + fn + ']' : fn;
                sbFields.Append(fieldName);
                sbFields.Append(",");

                var v = fvp.Item2;
                sbValues.Append(v);
                sbValues.Append(",");
            }
            if (sbFields.Length > 0)
            {
                sbFields.Remove(sbFields.Length - 1, 1);
                sbValues.Remove(sbValues.Length - 1, 1);
            }

            var cmd = $"insert into {tableName} ({sbFields.ToString()}) values ({sbValues.ToString()})";
            return cmd;
        }

        public static string CreateUpdate(string tableName, IList<(string, string)> fieldValuePairs, string cond)
        {
            var sbSetters = new StringBuilder();
            foreach (var fvp in fieldValuePairs)
            {
                var fn = fvp.Item1;
                var v = fvp.Item2;
                var fieldName = (fn.Contains(" ") && !fn.StartsWith("[")) ?
                    '[' + fn + ']' : fn;
                sbSetters.Append(fieldName);
                sbSetters.Append("=");
                sbSetters.Append(v);
                sbSetters.Append(",");
            }
            if (sbSetters.Length > 0)
            {
                sbSetters.Remove(sbSetters.Length - 1, 1);
            }
            var cmd = $"update {tableName} set {sbSetters.ToString()} where {cond}";
            return cmd;
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

        public static void RunNonQuery(this OleDbConnection conn, string cmdstr, bool closeConnectionOnComplete = true)
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
