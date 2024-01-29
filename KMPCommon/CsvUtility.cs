using System;
using System.Collections.Generic;
using System.IO;

namespace KMPCommon
{
    public static class CsvUtility
    {
        public enum BreakLineStatus
        {
            Normal,
            InQuote,
            ExitingQuote,
            NormalAfterQuote
        }

        public static IEnumerable<string> GetAndBreakRow(this StreamReader sr, bool trim = false)
        {
            var done = false;
            string? lastVal = null;
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
                            lastVal = lastVal + '\n' + val;
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
                            yield return lastVal!;
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

        public static IEnumerable<string?> BreakLine(this string line, bool trim = false, BreakLineStatus initialStatus = BreakLineStatus.Normal)
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
                        yield return trim ? lastItem.Trim() : lastItem;
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

        public static DateTime ParseDateTime(string fieldValue)
        {
            // TODO implement
            return DateTime.Parse(fieldValue);
        }
    }
}
