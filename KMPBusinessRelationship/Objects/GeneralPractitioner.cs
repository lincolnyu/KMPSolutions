namespace KMPBusinessRelationship.Objects
{
    public class GeneralPractitioner : Person
    {
        public string ProviderNumber { get; set; }      // Current Provider Number

        #region Mutable current values
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }       // Current primary phone number
        public string Fax { get; set; }
        #endregion
    }
}
