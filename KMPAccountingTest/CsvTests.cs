using KMPAccounting.BankAdapters;
using KMPAccounting.BookKeeping;
using KMPAccounting.KMPSpecifics;

namespace KMPAccountingTest
{
    public class CsvTests
    {
        private string TestConfig = Path.Combine(Utility.GetThisFolderPath(),  "localtest.cfg");
        private string TestDir;

        [SetUp]
        public void Setup()
        {
            if (!File.Exists(TestConfig))
            {
                Assert.Ignore();
            }
            using var cfg = new StreamReader(TestConfig);
            var cfgContent = cfg.ReadToEnd();
            var lines = cfgContent.Split('\n');
            var line = lines.First(x => x.StartsWith("csvtestdir="));
            var elems = line.Split('=');
            TestDir = elems[1];
        }

        [Test]
        public void CBATest()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacash"));
            var cbaCashCsv = dir.GetFiles().First();
            Assert.That(CsvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<CommbankCashRowDescriptor> ("CBA Cash")).Count, Is.EqualTo(179));
            Assert.Pass();
        }

        [Test]
        public void CBACashGuessTest()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacash"));
            var cbaCashCsv = dir.GetFiles().First();
            var rows = CsvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<CommbankCashRowDescriptor>("CBA Cash"));
            var guesser = new CommbankCashCounterAccountGuesser();
            var guessedRows = guesser.Guess(rows);
            {
                using var f = new StreamWriter(@"C:\temp\cbacash_guessed.csv");
                var emptyCount = 0;
                var filledCount = 0;
                foreach(var row in guessedRows)
                {
                    if (!string.IsNullOrWhiteSpace(row[row.OwnerTable.RowDescriptor.CounterAccountKey]))
                    {
                        filledCount++;
                    }
                    else
                    {
                        emptyCount++;
                    }
                    f.WriteLine(row);
                }
                var filledPercentage = (double)filledCount / (filledCount + emptyCount);
                Assert.That(filledPercentage, Is.GreaterThan(0.88));
            }
            Assert.Pass();
        }

        [Test]
        public void CBACCGuessTest()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacc"));
            var cbaCashCsv = dir.GetFiles().First();
            CsvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<CommbankCashRowDescriptor>("CBA Cash"));

            var rows = CsvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<CommbankCreditCardRowDescriptor>("CBA CreditCard")); 
            var guesser = new CommbankCreditCardCounterAccountGuesser();
            var guessedRows = guesser.Guess(rows);
            {
                using var f = new StreamWriter(@"C:\temp\cbacc_guessed.csv");
                var emptyCount = 0;
                var filledCount = 0;
                foreach (var row in guessedRows)
                {
                    if (!string.IsNullOrWhiteSpace(row[row.OwnerTable.RowDescriptor.CounterAccountKey]))
                    {
                        filledCount++;
                    }
                    else
                    {
                        emptyCount++;
                    }
                    f.WriteLine(row);
                }
                var filledPercentage = (double)filledCount / (filledCount + emptyCount);
                Assert.That(filledPercentage, Is.GreaterThan(0.88));
            }
            Assert.Pass();
        }
    }
}
