namespace KMPCommon
{
    public static class StringUtility
    {
        public static bool GetNextWord(this string line, char sep, int start, out int end, out string? word) 
        {
            end = line.IndexOf(sep, start);
            if (end == -1) { word = null; return false; }
            word = line[start..end];
            return true;
        }

        public static bool StartsWithCaseIgnored(this string str, string substr)
        {
            return str.StartsWith(substr, System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool ContainsCaseIgnored(this string str, string substr)
        {
            return str.Contains(substr, System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool ContainsWholeWord(this string str, string substr, bool ignoreCase = true)
        {
            for (var p = 0; p <= str.Length - substr.Length; p += substr.Length)
            {
                p = str.IndexOf(substr, p, ignoreCase ? System.StringComparison.OrdinalIgnoreCase : System.StringComparison.Ordinal);
                if (p >= 0)
                {
                    if (p > 0 && !char.IsWhiteSpace(str[p - 1])) continue;
                    if (p + substr.Length < str.Length && !char.IsWhiteSpace(str[p + substr.Length])) continue;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }
}
