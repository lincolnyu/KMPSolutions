using KMPAccounting.BankAdapters;
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
            var cbaCsvReader = new CommbankCashAccountAdapter.OriginalCsvReader(cbaCashCsv.FullName, "CBA Cash");
            Assert.That(cbaCsvReader.GetRows().Count, Is.EqualTo(179));
            Assert.Pass();
        }

        [Test]
        public void CBAGuessTest()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacash"));
            var cbaCashCsv = dir.GetFiles().First();
            var cbaCsvReader = new CommbankCashAccountAdapter.OriginalCsvReader(cbaCashCsv.FullName, "CBA Cash");
            var rows = cbaCsvReader.GetRows();
            var guesser = new CommbankCashCounterAccountGuesser();
            var guessedRows = guesser.Guess(rows);
            {
                using var f = new StreamWriter(@"C:\temp\cbacash_guessed.csv");
                foreach(var row in guessedRows)
                {
                    f.WriteLine(row);
                }
            }
            Assert.Pass();
        }
    }
}
