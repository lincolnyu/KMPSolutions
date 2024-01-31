using KMPAccounting.Objects.Accounts;
using KMPCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace KMPAccounting.Objects.BookKeeping
{
    public class PackedTransaction : Entry
    {
        public PackedTransaction(DateTime dateTime)
            : base(dateTime)
        {
        }

        public override bool Equals(Entry other)
        {
            if (other is PackedTransaction otherPt)
            {
                if (!CsvUtility.TimestampsAreEqual(DateTime, other.DateTime))
                {
                    return false;
                }
                if (!Equals(Remarks, otherPt.Remarks))
                {
                    return false;
                }
                if (Credited.Count != otherPt.Credited.Count)
                {
                    return false;
                }
                if (Debited.Count != otherPt.Debited.Count)
                {
                    return false;
                }
                for (var i =  0; i < Credited.Count; ++i)
                {
                    if (!Credited[i].Equals(otherPt.Credited[i]))
                    {
                        return false;
                    }
                }
                for (var i = 0; i < Debited.Count; ++i)
                {
                    if (!Debited[i].Equals(otherPt.Debited[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public static PackedTransaction ParseLine(DateTime dateTime, string line)
        {
            var pt = new PackedTransaction(dateTime);

            int p = 0;
            int newp;
            {
                line.GetNextWord('|', p, out newp, out string? creditedCountStr);
                p = newp + 1;
                var creditedCount = int.Parse(creditedCountStr);
                for (var i = 0; i < creditedCount; ++i)
                {
                    line.GetNextWord('|', p, out newp, out var credited);
                    p = newp + 1;
                    line.GetNextWord('|', p, out newp, out var amountStr);
                    p = newp + 1;
                    var amount = decimal.Parse(amountStr);
                    pt.Credited.Add((new AccountNodeReference(credited!), amount));
                }
            }

            {
                line.GetNextWord('|', p, out newp, out string? debitedCountStr);
                p = newp + 1;
                var debitedCount = int.Parse(debitedCountStr);
                for (var i = 0; i < debitedCount; ++i)
                {
                    line.GetNextWord('|', p, out newp, out var debited);
                    p = newp + 1;
                    line.GetNextWord('|', p, out newp, out var amountStr);
                    p = newp + 1;
                    var amount = decimal.Parse(amountStr);
                    pt.Debited.Add((new AccountNodeReference(debited!), amount));
                }
            }

            if (line.GetNextWord('|', p, out newp, out string? remarks))
            {
                pt.Remarks = remarks;
            }

            return pt;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(CsvUtility.TimestampToString(DateTime));
            sb.Append("|");
            sb.Append("PackedTransaction|");

            sb.Append($"{Credited.Count}|");
            foreach (var (acc, amount) in Credited)
            {
                sb.Append(acc.FullName);
                sb.Append("|");
                sb.Append(amount.ToString());
                sb.Append("|");
            }
            sb.Append($"{Debited.Count}|");
            foreach (var (acc, amount) in Debited)
            {
                sb.Append(acc.FullName);
                sb.Append("|");
                sb.Append(amount.ToString());
                sb.Append("|");
            }

            if (Remarks != null)
            {
                sb.Append(Remarks);
                sb.Append("|");
            }

            return sb.ToString();
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
