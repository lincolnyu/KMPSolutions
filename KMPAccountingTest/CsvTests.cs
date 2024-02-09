using KMPAccounting.BankSpecifics;
using KMPAccounting.BookKeeping;
using KMPAccounting.KMPSpecifics;
using System.Formats.Asn1;

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
        public void TestLoadingCBA()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacash"));
            var cbaCashCsv = dir.GetFiles().First(x=>x.Name == "CSVData.csv");
            var csvReader = new CsvReader();
            Assert.Multiple(() =>
            {
                Assert.That(csvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<CommbankCashRowDescriptor>("CBA Cash")).Count, Is.EqualTo(179));
                Assert.That(csvReader.HasHeader, Is.EqualTo(false));
            });
            Assert.Pass();
        }

        [Test]
        public void TestLoadingCBAWithHeader()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacash"));
            var cbaCashCsv = dir.GetFiles().First(x => x.Name == "CSVData_withheader.csv");
            var csvReader = new CsvReader();
            Assert.Multiple(() =>
            {
                Assert.That(csvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<CommbankCashRowDescriptor>("CBA Cash")).Count, Is.EqualTo(179));
                Assert.That(csvReader.HasHeader, Is.EqualTo(true));
            });
            Assert.Pass();
        }

        [Test]
        public void TetstCBACashGuess()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacash"));
            var cbaCashCsv = dir.GetFiles().First(x => x.Name == "CSVData_withheader.csv");
            var csvReader = new CsvReader();
            var rows = csvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<CommbankCashRowDescriptor>("CBA Cash"));

            var guesser = new CommbankCashCounterAccountPrefiller();
            var guessedRows = guesser.Guess(rows);
            {
                using var f = new StreamWriter(@"C:\temp\cbacash_guessed.csv");
                var headerChecked = false;
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
                    if (!headerChecked)
                    {
                        if (csvReader.HasHeader == true)
                        {
                            f.WriteLine(string.Join(',', csvReader.LoadedHeader!));
                        }
                        headerChecked = true;
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
            var cbaCashCsv = dir.GetFiles().First(x => x.Name == "CSVData_withheader.csv");

            var csvReader = new CsvReader();
            var rows = csvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<CommbankCreditCardRowDescriptor>("CBA CreditCard")); 
            var guesser = new CommbankCreditCardCounterAccountPrefiller();
            var guessedRows = guesser.Guess(rows);
            {
                using var f = new StreamWriter(@"C:\temp\cbacc_guessed.csv");
                if (csvReader.HasHeader == true)
                {
                    f.WriteLine(string.Join(',', csvReader.LoadedHeader!));
                }
                var headerChecked = false;
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
                    if (!headerChecked)
                    {
                        if (csvReader.HasHeader == true)
                        {
                            f.WriteLine(string.Join(',', csvReader.LoadedHeader!));
                        }
                        headerChecked = true;
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
