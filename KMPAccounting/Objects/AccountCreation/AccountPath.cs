﻿namespace KMPAccounting.Objects.AccountCreation
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
            suffix = suffix.Trim();
            if (suffix == "") return group.Path.TrimEnd('.');
            return $"{group.Path.TrimEnd('.')}.{suffix.TrimStart('.')}";
        }

        public bool StrictStartsWith(AccountPath accountPath)
        {
            return Path.StartsWith(accountPath.Path + ".");
        }

        public string GetRelative(string prefix)
        {
            var res = Path.Substring(prefix.Length);
            return res.TrimStart('.');
        }

        public override string ToString()
        {
            return Path;
        }

        public string Path { get; }
    }
}
