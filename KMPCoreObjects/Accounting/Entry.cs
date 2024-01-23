using System;

namespace KMPCoreObjects.Accounting
{
    public class Entry
    {
        public Entry(DateTime dateTime, Account credit, Account debit, decimal amount)
        {
            DateTime = dateTime;
            Credit = credit;
            Debit = debit;
            Amount = amount;
        }

        public DateTime DateTime { get; set; }
        public Account Credit { get; set; }
        public Account Debit { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
