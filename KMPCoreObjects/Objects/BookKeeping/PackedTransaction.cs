using KMPAccounting.Objects.Accounts;
using System;
using System.Collections.Generic;

namespace KMPAccounting.Objects.BookKeeping
{
    public class PackedTransaction : Entry
    {
        public PackedTransaction(DateTime dateTime)
            : base(dateTime)
        {
        }

        // The accounts being credited
        public List<(AccountNodeReference, decimal)> Credited { get; } = new List<(AccountNodeReference, decimal)>();

        // The accounts being debited
        public List<(AccountNodeReference, decimal)> Debited { get; } = new List<(AccountNodeReference, decimal)>();

        public string? Remarks { get; set; }

        public override void Redo()
        {
            foreach (var (credited, amount) in Credited)
            {
                var node = credited.Get()!;
                if (node.Side == AccountNode.SideEnum.Credit)
                {
                    node.Balance += amount;
                }
                else
                {
                    node.Balance -= amount;
                }
            }
            foreach (var (debited, amount) in Debited)
            {
                var node = debited.Get()!;
                if (node.Side == AccountNode.SideEnum.Debit)
                {
                    node.Balance += amount;
                }
                else
                {
                    node.Balance -= amount;
                }
            }
        }

        public override void Undo()
        {
            foreach (var (credited, amount) in Credited)
            {
                var node = credited.Get()!;
                if (node.Side == AccountNode.SideEnum.Credit)
                {
                    node.Balance -= amount;
                }
                else
                {
                    node.Balance += amount;
                }
            }
            foreach (var (debited, amount) in Debited)
            {
                var node = debited.Get()!;
                if (node.Side == AccountNode.SideEnum.Debit)
                {
                    node.Balance -= amount;
                }
                else
                {
                    node.Balance += amount;
                }
            }
        }
    }
}
