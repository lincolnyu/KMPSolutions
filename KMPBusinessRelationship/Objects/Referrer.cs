using System.Collections.Generic;

namespace KMPBusinessRelationship.Objects
{
    public class Referrer : Person
    {
        public string Id { get => ProviderNumber; set => ProviderNumber = value; }

        /// <summary>
        ///  An ID that identifies referrer.
        /// </summary>
        public string ProviderNumber { get; set; }

        public HashSet<string> OtherProviderNumbers { get; } = new HashSet<string>();

        #region Mutable current values

        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }       // Current primary phone number
        public string Fax { get; set; }

        #endregion
    }
}
