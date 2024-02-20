using System;
using System.Collections.Generic;
using System.Linq;
using KMPAccounting.Objects.Fundamental;

namespace KMPAccounting.Objects.Accounts
{
    public class AccountNode : WeakPointable<AccountNode>
    {
        public enum SideEnum
        {
            Debit,
            Credit
        }

        public static SideEnum GetOppositeSide(SideEnum side)
        {
            return side == SideEnum.Credit? SideEnum.Debit : SideEnum.Credit;
        }

        public AccountNode(SideEnum side, string name)
        {
            Side = side;
            Name = name;
        }

        public override void Dispose()
        {
            foreach (var c in Children.Values)
            {
                c.Dispose();
            }
            base.Dispose();
        }

        public SideEnum Side { get; }

        public string Name { get; set; }

        // Note: This uniquely identifies the account node systemwide.
        public string FullName => Parent != null ? Parent.FullName + "." + Name : Name;

        public AccountNode? Parent { get; set; }
        public Dictionary<string, AccountNode> Children { get; } = new Dictionary<string, AccountNode>();

        public decimal Balance
        {
            get
            {
                if (Children.Count > 0)
                {
                    if (balanceInvalidated_)
                    {
                        balance_ = Children.Values.Sum(x => SameAccountSide(x) ? x.Balance : -x.Balance);
                        balanceInvalidated_ = false;
                    }
                }
                return balance_;
            }
            set
            {
                if (Children.Count > 0)
                {
                    throw new InvalidOperationException("Setting balance to an aggregate account is not allowed");
                }

                if (balance_ != value)
                {
                    balance_ = value;

                    InvalidateParentsBalances();
                }
            }
        }

        public bool SameAccountSide(AccountNode that) => Side == that.Side;

        public bool IsSameAccountAs(AccountNode that) => FullName == that.FullName && Side == that.Side;

        // Assuming that node has already the same side and name
        public void CopyTo(AccountNode that, bool exactCopy)
        {
            CopyBasicFieldsTo(that);
            foreach (var (childName, child) in Children)
            {
                if (!that.Children.TryGetValue(childName, out var thatChild))
                {
                    thatChild = new AccountNode(child.Side, childName)
                    {
                        Parent = that,
                    };
                    that.Children.Add(childName, thatChild);
                }
                child.CopyTo(thatChild, exactCopy);
            }

            foreach (var (thatChildName, thatChild) in that.Children)
            {
                if (!Children.ContainsKey(thatChildName))
                {
                    if (exactCopy)
                    {
                        that.Children.Remove(thatChildName);
                        thatChild.Dispose();
                    }
                    else
                    {
                        thatChild.ZeroOutBalanceOfTree();
                    }
                }
            }
        }

        /// <summary>
        ///  Force the balance of every node on the tree starting from this account to zero.
        /// </summary>
        /// <remarks>
        ///  This is not a balanced accounting operation.
        /// </remarks>
        public void ZeroOutBalanceOfTree()
        {
            var needToInvalidateParentsBalances = balance_ != 0;

            ZeroOutBalanceOfTreeWithoutInvalidatingParents();

            if (needToInvalidateParentsBalances)
            {
                InvalidateParentsBalances();
            }
        }

        private void InvalidateParentsBalances()
        {
            for (var p = Parent; p != null; p = p.Parent)
            {
                p.balanceInvalidated_ = true;
            }
        }

        private void ZeroOutBalanceOfTreeWithoutInvalidatingParents()
        {
            balance_ = 0;
            balanceInvalidated_ = false;
            foreach (var child in Children.Values)
            {
                child.ZeroOutBalanceOfTreeWithoutInvalidatingParents();
            }
        }

        private void CopyBasicFieldsTo(AccountNode that)
        {
            that.balance_ = balance_;
            that.balanceInvalidated_ = balanceInvalidated_;
        }

        private decimal balance_ = 0;
        private bool balanceInvalidated_ = false;
    }
}
