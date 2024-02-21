using KMPAccounting.Objects.Accounts;
using KMPAccounting.Objects.BookKeeping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace KMPAccounting.Objects
{
    public static class Utility
    {
        public static AccountNode? GetAccount(string fullName)
        {
            var split = fullName.Split('.', 2);
            Debug.Assert(split.Length <= 2);
            var state = AccountsState.GetAccountsState(split[0]);
            if (state == null) return null;
            if (split.Length == 1)
            {
                return state;
            }
            return state.GetAccount(split[1]);
        }

        public static AccountNode? GetAccount(this AccountsState state, string fullName)
        {
            var splitNames = fullName.Split('.');
            AccountNode p = state;
            foreach (var splitName in splitNames)
            {
                if (!p.Children.TryGetValue(splitName, out var account))
                {
                    return null;
                }
                p = account;
            }
            return p;
        }

        public static void ReckonAccountByTransactions(this AccountNode node, out List<(string, decimal)> toDebit, out List<(string, decimal)> toCredit)
        {
            toDebit = new List<(string, decimal)>();
            toCredit = new List<(string, decimal)>();
            decimal netDebited = 0;
            foreach (var leaf in GetNonZeroLeafNodesForReckoning(node))
            {
                var leafPositiveBalance = leaf.Balance > 0;
                var leafDebitSide = leaf.Side == AccountNode.SideEnum.Debit;
                var amount = Math.Abs(leaf.Balance);
                if (leafPositiveBalance ^ leafDebitSide)
                {
                    // Debit leaf and credit base
                    toDebit.Add((leaf.FullName, amount));
                    netDebited += amount;
                }
                else
                {
                    // Credit leaf and debit base
                    toCredit.Add((leaf.FullName, amount));
                    netDebited -= amount;
                }
            }
            // There's a chance these accounts cancel themselves out
            if (netDebited > 0)
            {
                toCredit.Add((node.BaseNode!.FullName, netDebited));
            }
            else if (netDebited < 0)
            {
                toDebit.Add((node.BaseNode!.FullName, -netDebited));
            }
        }

        private static IEnumerable<AccountNode> GetNonZeroLeafNodesForReckoning(AccountNode node)
        {
            foreach (var child in node.Children.Values.Where(x => x != node.BaseNode))
            {
                if (child.Children.Count > 0)
                {
                    foreach (var l in GetNonZeroLeafNodesForReckoning(child))
                    {
                        yield return l;
                    }
                }
                else if (child.Balance != 0)
                {
                    yield return child;
                }
            }
        }

        /// <summary>
        ///  Ensure the specified account is created in the specified state by executing the OpenAccount entries it creates as required.
        /// </summary>
        /// <param name="ledger">The ledger to use for the account opening entry.</param>
        /// <param name="state">The accounts state the account is in.</param>
        /// <param name="fullName">The full name that identify the account in the state.</param>
        /// <param name="addingToLedger">If the open account entry is to be added to the ledger.</param>
        public static void EnsureCreateAccount(this AccountsState state, string fullName, bool sideDifferToParent, Ledger? ledgerToAddTo)
        {
            var splitNames = fullName.Split('.');
            AccountNode p = state;
            string? parentFullName = null;
            var i = 0;
            foreach (var seg in splitNames)
            {
                if (parentFullName != null || !p.Children.TryGetValue(seg, out var child))
                {
                    if (parentFullName == null)
                    {
                        parentFullName = p.FullName;
                    }
                    var side = i == splitNames.Length - 1 && sideDifferToParent ? AccountNode.GetOppositeSide(p.Side) : p.Side;
                    var openAccount = new OpenAccount(DateTime.Now, (new AccountNodeReference(parentFullName), side), seg);
                    if (ledgerToAddTo != null)
                    {
                        ledgerToAddTo.Entries.Add(openAccount);
                    }
                    openAccount.Redo();
                    parentFullName += "." + seg;
                }
                else
                {
                    p = child;
                }
                ++i;
            }
        }

        /// <summary>
        ///  Ensure the specified account is created in an existing state as specified in its full name.
        /// </summary>
        /// <param name="ledger">The ledger to use for the account opening entry.</param>
        /// <param name="fullName">The full name that identify the account globally.</param>
        /// <param name="sideDifferToParent">If the account's leaf node is to have the opposite side to its immediate parent. The intermediate nodes created towards the leaf node will all have the same side as their parents.</param>
        public static void EnsureCreateAccount(this Ledger ledger, string fullName, bool sideDifferToParent)
        {
            // This makes sure the state is already created.
            // The function is meant to create accounts for a state. And that's why it expects the fullName to have at least 2 segments.
            var split = fullName.Split('.', 2);

            var stateName = split[0];
            var state = AccountsState.GetAccountsState(stateName);
            if (state == null)
            {
                var openAccount = new OpenAccount(DateTime.Now, null, stateName);
                ledger.Entries.Add(openAccount);
                openAccount.Redo();

                state = AccountsState.GetAccountsState(stateName)!;
            }

            if (split.Length == 2)
            {
                state.EnsureCreateAccount(split[1], sideDifferToParent, ledger);
            }
        }

        public static void AddAndExecuteTransaction(this Ledger ledger, DateTime dateTime, string debitedAccountFullName, string creditedAccountFullName, decimal amount, string? remarks = null)
        {
            var transaction = new SimpleTransaction(dateTime, new AccountNodeReference(debitedAccountFullName), new AccountNodeReference(creditedAccountFullName), amount) { Remarks = remarks };
            ledger.Entries.Add(transaction);
            transaction.Redo();
        }

        public static void AddAndExecuteTransaction(this Ledger ledger, DateTime dateTime, IEnumerable<(string, decimal)> debited, IEnumerable<(string, decimal)> credited, string? remarks = null)
        {
            var transaction = new CompositeTransaction(dateTime)
            {
                Remarks = remarks
            };
            foreach (var (accountFullName, amount) in debited)
            {
                transaction.Debited.Add((new AccountNodeReference(accountFullName), amount));
            }
            foreach (var (accountFullName, amount) in credited)
            {
                transaction.Credited.Add((new AccountNodeReference(accountFullName)!, amount));
            }
            // TODO Add balance checking assert.
            ledger.Entries.Add(transaction);
            transaction.Redo();
        }
    }
}
