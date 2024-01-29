using System.Collections.Generic;

namespace KMPAccounting.Objects.Accounts
{
    public class AccountsState : AccountNode
    {
        public AccountsState(string name)
            : base(SideEnum.Credit, name)
        {
        }

        public static AccountsState? GetAccountsState(string name)
        {
            if (accountStates_.TryGetValue(name, out AccountsState state))
            {
                return state;
            }
            return null;
        }

        public static void AddState(string name, AccountsState state)
        {
            accountStates_.Add(name, state);
        }

        public static bool RemoveState(string name)
        {
            var result = accountStates_.Remove(name, out var removed);
            removed.Dispose();
            return result;
        }

        private static readonly Dictionary<string, AccountsState> accountStates_ = new Dictionary<string, AccountsState>();
    }
}
