namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class WaveRowDescriptor : GenericInvoiceRowDescriptor
    {
        public WaveRowDescriptor()
        {
            Keys.Add(WaveAccountKey);
            Keys.Add(WaveCategoryKey);
            Keys.Add(WaveDescriptionKey);
        }

        public string WaveAccountKey { get; } = "Wave Account";
        public string WaveCategoryKey { get; } = "Wave Category";
        public string WaveDescriptionKey { get; } = "Wave Description";
    }
}
