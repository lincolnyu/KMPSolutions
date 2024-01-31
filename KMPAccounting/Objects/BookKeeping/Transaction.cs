using System;
using System.Text;
using KMPAccounting.Objects.Accounts;
using KMPCommon;

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

        public override bool Equals(Entry other)
        {
            if (other is Transaction otherT)
            {
                if (!CsvUtility.TimestampsAreEqual(DateTime, other.DateTime))
                {
                    return false;
                }
                if (!Credited.Equals(otherT.Credited))
                {
                    return false;
                }
                if (!Debited.Equals(otherT.Debited))
                {
                    return false;
                }
                if (!Amount.Equals(otherT.Amount))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public static Transaction ParseLine(DateTime dateTime, string line)
        {
            int p = 0;
            int newp;

            line.GetNextWord('|', p, out newp, out var creditedAccountName);
            p = newp + 1;

            line.GetNextWord('|', p, out newp, out var debitedAccountName);
            p = newp + 1;

            line.GetNextWord('|', p, out newp, out var amountStr);
            p = newp + 1;
            var amount = decimal.Parse(amountStr);

            line.GetNextWord('|', p, out newp, out string? remarks);

            return new Transaction(dateTime, new AccountNodeReference(creditedAccountName!), new AccountNodeReference(debitedAccountName!), amount)
            {
                Remarks = remarks
            };
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(CsvUtility.TimestampToString(DateTime));
            sb.Append("|");
            sb.Append("Transaction|");

            sb.Append(Credited.FullName);
            sb.Append("|");

            sb.Append(Debited.FullName);
            sb.Append("|");

            sb.Append(Amount.ToString());
            sb.Append("|");

            if (Remarks != null)
            {
                sb.Append(Remarks);
                sb.Append("|");
            }

            return sb.ToString();
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
