using System.Runtime.CompilerServices;

namespace KMPAccountingTest
{
    public static class Utility
    {
        public static string GetThisFolderPath([CallerFilePath] string path = null)
        {
            return Path.GetDirectoryName(path);
        }

        public static string GetThisFilePath([CallerFilePath] string path = null)
        {
            return path;
        }
    }
}
