﻿using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace KmpCrmCore
{
    public static class CsvUtil
    {
        public enum BreakLineStatus
        {
            Normal,
            InQuote,
            ExitingQuote,
            NormalAfterQuote
        }

        public static IEnumerable<string> GetAndBreakRow(this StreamReader sr, bool trim=false)
        {
            var done = false;
            string lastVal = null;
            var status = BreakLineStatus.Normal;
            while (!done)
            {
                var line = sr.ReadLine();
                if (line == null)
                {
                    break;
                }
                var fieldVals = line.BreakLine(trim, status);
                var first = true;
                done = true;
                foreach (var val in fieldVals)
                {
                    if (first)
                    {
                        if (lastVal != null)
                        {
                            lastVal = lastVal + val;
                        }
                        else
                        {
                            lastVal = val;
                        }
                        first = false;
                    }
                    else
                    {
                        if (val == null)
                        {
                            done = false;
                            status = BreakLineStatus.InQuote;
                            break;
                        }
                        else
                        {
                            yield return lastVal;
                            lastVal = val;
                        }
                    }
                }
            }
            if (lastVal != null)
            {
                yield return lastVal;
            }
        }

        public static IEnumerable<string> BreakLine(this string line, bool trim = false, BreakLineStatus initialStatus = BreakLineStatus.Normal)
        {
            BreakLineStatus status = initialStatus;
            string lastItem = "";
            foreach (var ch in line)
            {
                if (status == BreakLineStatus.Normal || status == BreakLineStatus.NormalAfterQuote)
                {
                    if (ch == ',')
                    {
                        yield return trim ? lastItem.Trim() : lastItem;
                        lastItem = "";
                        status = BreakLineStatus.Normal;
                    }
                    else if (ch == '"' && lastItem == "")
                    {
                        status = BreakLineStatus.InQuote;
                    }
                    else
                    {
                        lastItem += ch;
                    }
                }
                else if (status == BreakLineStatus.InQuote)
                {
                    if (ch == '"')
                    {
                        status = BreakLineStatus.ExitingQuote;
                    }
                    else
                    {
                        lastItem += ch;
                    }
                }
                else // status == BreakLineStatus.ExitingQuote
                {
                    if (ch == '"')
                    {
                        lastItem += '"';
                        status = BreakLineStatus.InQuote;
                    }
                    else if (ch == ',')
                    {
                        yield return trim ? lastItem.Trim(): lastItem;
                        lastItem = "";
                        status = BreakLineStatus.Normal;
                    }
                    else
                    {
                        lastItem += ch;
                        status = BreakLineStatus.NormalAfterQuote;
                    }
                }
            }
            if (lastItem != "")
            {
                yield return trim ? lastItem.Trim() : lastItem;
            }
            if (status == BreakLineStatus.InQuote)
            {
                yield return null;
            }
        }

        public static CommentedValue<int?> ParseInt(this string field, bool trimComment = false)
        {
            field = field.Trim();

            var i = 0;
            var res = 0;
            for (; i < field.Length; i++)
            {
                var ch = field[i];
                if (char.IsDigit(ch))
                {
                    res *= 10;
                    res += ch - '0';
                }
            }
            var comment = field[i..];
            if (trimComment)
            {
                comment = comment.Trim();
            }
            return new CommentedValue<int?>(i > 0 ? res : null, comment);
        }

        public static CommentedValue<bool>  ParseYes(this string field, bool trimComment = false)
        {
            field = field.Trim();

            var yes = field.Length > 0 && char.ToUpper(field[0]) == 'Y'
                && (field.Length == 1 || !char.IsLetter(field[1]));
            var comment = field.Substring(yes ? 1 : 0);
            if (trimComment)
            {
                comment = comment.Trim();
            }
            return new CommentedValue<bool>(yes, comment);
        }

        public static string CsvEscape(this string original)
        {
            if (original.Contains(',') || original.Contains('"'))
            {
                var n = original.Replace("\"", "\"\"");
                return $"\"{n}\"";
            }
            return original;
        }

        public static string ToCsvField(this CommentedValue<bool> value, bool omitFalse)
        {
            if (value.HasComments)
            {
                var s = value.Value ? "Y - " : "N - ";
                s += value.Comments;
                return s.CsvEscape();
            }
            else
            {
                if (value.Value || !omitFalse)
                {
                    return value.Value ? "Y" : "N";
                }
                return "";
            }
        }

        public static string VisitsToCsvField(this List<CommentedValue<VisitBatch>> visitBatches)
        {
            var sb = new StringBuilder();
            foreach (var cvb in visitBatches)
            {
                var vb = cvb.Value;
                sb.Append($"{vb.ExpectedVisits}EVs:");
                var notFirst = false;
                foreach (var vm in vb.VisitsMade)
                {
                    if (notFirst)
                    {
                        sb.Append(";");
                    }
                    sb.Append($"{vm.Value.DateToString()}");
                    if (vm.HasComments)
                    {
                        sb.Append("-");
                        sb.Append(vm.Comments);
                    }
                    notFirst = true;
                }
                if (cvb.HasComments)
                {
                    sb.Append(":");
                    sb.Append(cvb.Comments);
                }
            }
            return sb.ToString().CsvEscape();
        }

        public static string DateToString(this DateOnly date)
        {
            return date.ToShortDateString();
        }
    }
}
