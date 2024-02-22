namespace KMPAccounting.BookKeepingTabular
{
    public class AccountGroup
    {
        public AccountGroup(string path)
        {
            Path = path;
        }

        public static implicit operator AccountGroup(string path)
        {
            return new AccountGroup(path);
        }

        public string Account(string suffix)
        {
            return $"{Path.TrimEnd('.')}.{suffix.TrimStart('.')}";
        }

        public static string operator +(AccountGroup group, string suffix)
        {
            return group.Account(suffix);
        }

        public string Path { get; }
    }
}
