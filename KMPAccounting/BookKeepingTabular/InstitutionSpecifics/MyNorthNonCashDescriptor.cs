using KMPAccounting.BookKeepingTabular;
using KMPCommon;
using System;

namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class MyNorthNonCashDescriptor : SuperRowDescriptor
    {
        /// <summary>
        ///  Constructor. Using precise column names have the benefits such as being able to detect header.
        /// </summary>
        public MyNorthNonCashDescriptor() : base("Trade Date", "Amount ($)", new[] { "Trade Date", "Settlement Date", "Transaction Type", "Investment", "Description", "Quantity", "Unit Price", "Amount ($)" })
        {
        }

        public static decimal? GetUnitPrice(ITransactionRow row) => row.GetDecimalValue("Unit Price");
        public static decimal? GetQuantity(ITransactionRow row) => row.GetDecimalValue("Quantity");

        public static DateTime GetSettlementDate(ITransactionRow row) => CsvUtility.ParseDateTime(row["Settlement Date"]);

        public static string GetTransactionType(ITransactionRow row) => row["Transaction Type"];
        public static string GetDescription(ITransactionRow row) => row["Description"];
        public static string GetInvestment(ITransactionRow row) => row["Investment"];
    }
}

