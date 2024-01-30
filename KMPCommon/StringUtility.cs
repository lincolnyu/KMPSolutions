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
    }
}
