﻿using System;
using System.Text;
using KMPAccounting.Objects.Accounts;
using KMPCommon;

namespace KMPAccounting.Objects.BookKeeping
{
    public class SimpleTransaction : Entry
    {
        public SimpleTransaction(DateTime dateTime, AccountNodeReference debited, AccountNodeReference credited, decimal amount)
            : base(dateTime)
        {
            Debited = debited;
            Credited = credited;
            Amount = amount;
        }

        public override bool Equals(Entry other)
        {
            if (other is SimpleTransaction otherT)
            {
                if (!CsvUtility.TimestampsAreEqual(DateTime, other.DateTime))
                {
                    return false;
                }
                if (!Debited.Equals(otherT.Debited))
                {
                    return false;
                }
                if (!Credited.Equals(otherT.Credited))
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

        public static SimpleTransaction ParseLine(DateTime dateTime, string line)
        {
            int p = 0;

            line.GetNextWord('|', p, out int newp, out var debitedAccountName);
            p = newp + 1;

            line.GetNextWord('|', p, out newp, out var creditedAccountName);
            p = newp + 1;

            line.GetNextWord('|', p, out newp, out var amountStr);
            p = newp + 1;
            var amount = decimal.Parse(amountStr);

            line.GetNextWord('|', p, out _, out string? remarks);

            return new SimpleTransaction(dateTime, new AccountNodeReference(debitedAccountName!), new AccountNodeReference(creditedAccountName!), amount)
            {
                Remarks = remarks
            };
        }

        public override string SerializeToLine()
        {
            var sb = new StringBuilder();

            sb.Append(CsvUtility.TimestampToString(DateTime));
            sb.Append("|");
            sb.Append("Transaction|");

            sb.Append(Debited.FullName);
            sb.Append("|");

            sb.Append(Credited.FullName);
            sb.Append("|");

            sb.Append(Amount.ToString());
            sb.Append("|");

            if (!string.IsNullOrEmpty(Remarks))
            {
                sb.Append($"Remarks: {Remarks}" );
                sb.Append("|");
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(DateTime.ToShortDateOnlyString());
            sb.AppendLine("Debit");
            sb.AppendLine($"  {Amount} to {Debited.FullName}");

            sb.AppendLine("Credit");
            sb.AppendLine($"  {Amount} to {Credited.FullName}");

            if (Remarks != null)
            {
                sb.AppendLine($"Remarks: {Remarks}");
            }

            return sb.ToString();
        }

        // The account being debited
        public AccountNodeReference Debited { get; set; }

        // The acount being credited
        public AccountNodeReference Credited { get; set; }

        public decimal Amount { get; set; }

        public override void Redo()
        {
            var debitedNode = Debited.Get()!;
            if (debitedNode.Side == AccountNode.SideEnum.Debit)
            {
                debitedNode.Balance += Amount;
            }
            else
            {
                debitedNode.Balance -= Amount;
            }

            var creditedNode = Credited.Get()!;
            if (creditedNode.Side == AccountNode.SideEnum.Credit)
            {
                creditedNode.Balance += Amount;
            }
            else
            {
                creditedNode.Balance -= Amount;
            }
        }

        public override void Undo()
        {
            var debitedNode = Debited.Get()!;
            if (debitedNode.Side == AccountNode.SideEnum.Debit)
            {
                debitedNode.Balance -= Amount;
            }
            else
            {
                debitedNode.Balance += Amount;
            }

            var creditedNode = Credited.Get()!;
            if (creditedNode.Side == AccountNode.SideEnum.Credit)
            {
                creditedNode.Balance -= Amount;
            }
            else
            {
                creditedNode.Balance += Amount;
            }
        }
    }
}
