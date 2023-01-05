using System.Net.WebSockets;
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
            var explicitNo = field.Length > 0 && char.ToUpper(field[0]) == 'N'
                && (field.Length == 1 || !char.IsLetter(field[1]));
            var hasYesOrNo = yes || explicitNo;
            var comment = field.Substring(hasYesOrNo ? 1 : 0);
            if (trimComment)
            {
                comment = comment.Trim();
            }
            else if (hasYesOrNo)
            {
                comment = comment.TrimStart();
            }
            if (comment.StartsWith('-'))
            {
                comment = comment.Substring(1);
            }
            return new CommentedValue<bool>(yes, comment);
        }

        public static string CsvEscape(this string original)
        {
            if (original.Contains(',') || original.Contains('"') || original.Contains('\n'))
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
            var ftt = true;
            foreach (var cvb in visitBatches)
            {
                if (!ftt)
                {
                    sb.AppendLine();
                }
                ftt = false;
                var vb = cvb.Value;
                sb.Append($"{vb.ExpectedVisits}|");
                var ftt2 = true;
                foreach (var vm in vb.VisitsMade.OrderBy(x=>x.Value))
                {
                    if (!ftt2)
                    {
                        sb.Append(";");
                    }
                    sb.Append($"{vm.Value.DateToString()}");
                    if (vm.HasComments)
                    {
                        sb.Append("-");
                        sb.Append(vm.Comments);
                    }
                    ftt2 = false;
                }
                sb.Append("|");
                ftt2 = true;
                foreach (var cm in vb.ClaimsMade.OrderBy(x => x.Value))
                {
                    if (!ftt2)
                    {
                        sb.Append(";");
                    }
                    sb.Append($"{cm.Value.DateToString()}");
                    if (cm.HasComments)
                    {
                        sb.Append("-");
                        sb.Append(cm.Comments);
                    }
                    ftt2 = false;
                }
                sb.Append("|");
                if (cvb.HasComments)
                {
                    sb.Append("|");
                    sb.Append(cvb.Comments);
                }
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
                if (split.Length != 4)
                {
                    // TODO: Report error.
                    continue;
                }
                int? expectedVisits = null;
                if (!string.IsNullOrEmpty(split[0]))
                {
                    if (!int.TryParse(split[0], out var ev))
                    {
                        // TODO: Report error.
                        continue;
                    }
                    expectedVisits = ev;
                }
                var comments = split[3];
                var vb = new CommentedValue<VisitBatch>(new VisitBatch { ExpectedVisits = expectedVisits }, comments);
                var visitsSplit = split[1].Split(";", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                foreach (var visit in visitsSplit.StringsToEnumerableOfCommented<DateOnly>(DateOnly.TryParse))
                {
                    vb.Value.VisitsMade.Add(visit);
                }
                var claimsSplit = split[2].Split(";", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                foreach (var claim in claimsSplit.StringsToEnumerableOfCommented<DateOnly>(DateOnly.TryParse))
                {
                    vb.Value.ClaimsMade.Add(claim);
                }
                yield return vb;
            }
        }

        public delegate bool TryParse<T>(string arg1, out T arg2);
        public static IEnumerable<CommentedValue<T>> StringsToEnumerableOfCommented<T>(this IEnumerable<string> strings, TryParse<T> tryParse, char commentSeparator='-', bool ignoreError=true)
        {
            var hasError = false;
            foreach (var str in strings)
            {
                var split = str.Split(commentSeparator);
                if (!tryParse(split[0], out T val))
                {
                    hasError = true;
                }
                var item = new CommentedValue<T>(val);
                if (split.Length > 1)
                {
                    item.Comments = split[1];
                }
                yield return item;
            }
            if (!ignoreError && hasError)
            {
                throw new ArgumentException("Bad input strings to convert to enumerable of commented.");
            }
        }

        public static string DateToString(this DateOnly date)
        {
            return date.ToShortDateString();
        }
    }
}
