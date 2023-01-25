using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KmpCrmCore
{
    using DateOnly = System.DateTime;

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
            var comment = field.Substring(i);
            if (trimComment)
            {
                comment = comment.Trim();
            }
            return new CommentedValue<int?>(i > 0 ? res : (int?)null, comment);
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
            if (original.Contains(",") || original.Contains("\""))
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
                sb.Append($"{vb.ExpectedVisits}|");
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
                    sb.Append("|");
                    sb.Append(cvb.Comments);
                }
                sb.AppendLine();
            }
            return sb.ToString().CsvEscape();
        }

        public static IEnumerable<CommentedValue<VisitBatch>> CsvFieldToVisits(this string visitsField)
        {
            var sr = new StringReader(visitsField);
            while (true)
            {
                var line = sr.ReadLine();
                if (line == null) break;
                var split = line.Split('|');
                if (split.Length != 3)
                {
                    // TODO: Report error.
                    continue;
                }
                if (!int.TryParse(split[0], out var expectedVisits))
                {
                    // TODO: Report error.
                    continue;
                }
                var comments = split[2];
                var vb = new CommentedValue<VisitBatch>(new VisitBatch { ExpectedVisits = expectedVisits }, comments);
                var visitsSplit = split[1].Split(';');
                foreach (var visitStr in visitsSplit)
                {
                    var visitStrSplit = visitStr.Split('-');
                    if (!DateOnly.TryParse(visitStrSplit[0], out var date))
                    {
                        // TODO: handle it..
                    }
                    var visit = new CommentedValue<DateOnly>(date);
                    if (visitStrSplit.Length > 1)
                    {
                        visit.Comments = visitsSplit[1];
                    }
                    vb.Value.VisitsMade.Add(visit);
                }
                yield return vb;
            }

        }

        public static string DateToString(this DateOnly date)
        {
            return date.ToShortDateString();
        }
    }
}
