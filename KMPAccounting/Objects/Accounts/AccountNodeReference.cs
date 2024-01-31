using KMPAccounting.Objects.Fundamental;
using System;

namespace KMPAccounting.Objects.Accounts
{
    public class AccountNodeReference : IEquatable<AccountNodeReference>
    {
        public AccountNodeReference(string fullName)
        {
            FullName = fullName;
        }

        public bool Equals(AccountNodeReference other)
        {
            return FullName.Equals(other.FullName);
        }

        public override string ToString()
        {
            return FullName;
        }

        // Including the states name as the root
        public string FullName { get; }

        public AccountNode? Get()
        {
            if (nodeCache_ != null && nodeCache_.TryGetTarget(out var node))
            {
                return node;
            }
            var account = Utility.GetAccount(FullName);
            if (account != null)
            {
                nodeCache_ = new WeakPointer<AccountNode>(account);
            }
            return account;
        }


        WeakPointer<AccountNode>? nodeCache_ = null;
    }
}
