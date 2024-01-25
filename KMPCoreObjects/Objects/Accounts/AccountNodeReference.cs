using KMPAccounting.Objects.Fundamental;

namespace KMPAccounting.Objects.Accounts
{
    public class AccountNodeReference
    {
        public AccountNodeReference(string fullName)
        {
            FullName = fullName;
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
