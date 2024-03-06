namespace KMPAccounting.BookKeepingTabular
{
    public class AccountPath
    {
        public AccountPath(string path)
        {
            Path = path;
        }

        public static implicit operator AccountPath(string path)
        {
            return new AccountPath(path);
        }

        public static implicit operator string(AccountPath accountGroup)
        {
            return accountGroup.Path;
        }

        public static AccountPath operator +(AccountPath group, string suffix)
        {
            return $"{group.Path.TrimEnd('.')}.{suffix.TrimStart('.')}";
        }

        public override string ToString()
        {
            return Path;
        }

        public string Path { get; }
    }
}
