using System.Text.Json;

namespace KmpCrmCore
{
    public static class CsvUtil
    {
        enum BreakLineStatus
        {
            Normal,
            InQuote,
            ExitingQuote,
            NormalAfterQuote
        }

        public static IEnumerable<string> BreakLine(this string line, bool trim = false)
        {
            BreakLineStatus status = BreakLineStatus.Normal;
            string lastItem = "";
            foreach (var ch in line)
            {
                if (status == BreakLineStatus.Normal || status == BreakLineStatus.NormalAfterQuote)
                {
                    if (ch == ',')
                    {
                        yield return trim ? lastItem : lastItem.Trim();
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
                        yield return trim ? lastItem : lastItem.Trim();
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
                yield return trim ? lastItem : lastItem.Trim();
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
    }
}
