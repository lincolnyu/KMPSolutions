using System;
using KMPAccounting.Objects.Accounts;

namespace KMPAccounting.Objects.BookKeeping
{
    public class Transaction : Entry
    {
        public Transaction(DateTime dateTime, AccountNodeReference credited, AccountNodeReference debited, decimal amount)
            : base(dateTime)
        {
            Credited = credited;
            Debited = debited;
            Amount = amount;
        }

        // The acount being credited
        public AccountNodeReference Credited { get; set; }

        // The account being debited
        public AccountNodeReference Debited { get; set; }

        public decimal Amount { get; set; }

        public string? Remarks { get; set; }

        public override void Redo()
        {
            var creditedNode = Credited.Get()!;
            if (creditedNode.Side == AccountNode.SideEnum.Credit)
            {
                creditedNode.Balance += Amount;
            }
            else
            {
                creditedNode.Balance -= Amount;
            }

            var debitedNode = Debited.Get()!;
            if (debitedNode.Side == AccountNode.SideEnum.Debit)
            {
                debitedNode.Balance += Amount;
            }
            else
            {
                debitedNode.Balance -= Amount;
            }
        }

        public override void Undo()
        {
            var creditedNode = Credited.Get()!;
            if (creditedNode.Side == AccountNode.SideEnum.Credit)
            {
                creditedNode.Balance -= Amount;
            }
            else
            {
                creditedNode.Balance += Amount;
            }

            var debitedNode = Debited.Get()!;
            if (debitedNode.Side == AccountNode.SideEnum.Debit)
            {
                debitedNode.Balance -= Amount;
            }
            else
            {
                debitedNode.Balance += Amount;
            }
        }
    }
}
