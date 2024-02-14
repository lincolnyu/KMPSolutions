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

        public string WaveAccountKey { get; } = "WaveAccount";
        public string WaveCategoryKey { get; } = "WaveCategory";
        public string WaveDescriptionKey { get; } = "WaveDescription";
    }
}
