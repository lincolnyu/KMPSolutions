using System.Collections.Generic;
using System.Linq;

namespace KMPBusinessRelationship.Objects
{
    public class Referrer : Person
    {
        public IEnumerable<string> Ids 
        { 
            get
            {
                yield return ProviderNumber;
                foreach (var pn in OtherProviderNumbers)
                {
                    yield return pn;
                }
            }
        }

        public string PrimaryId => ProviderNumber;

        /// <summary>
        ///  An ID that identifies referrer.
        /// </summary>
        public string ProviderNumber { get; set; }

        public string OtherProviderNumberList
        {
            get => string.Join(',', OtherProviderNumbers);
            set
            {
                OtherProviderNumbers.Clear();
                var valueSplit = value.Split(',');
                foreach (var v in valueSplit)
                {
                    OtherProviderNumbers.Add(v);
                }
            }
        }
        public HashSet<string> OtherProviderNumbers { get; } = new HashSet<string>();

        #region Mutable current values

        public string Name { get; set; }
        public string Phone { get; set; }       // Current primary phone number
        public string Fax { get; set; } = "";
        public string PracticeName { get; set; } = "";
        public string Address { get; set; } = "";
        public string PostalAddress { get; set; } = "";
        public string Remarks { get; set; } = "";

        #endregion
    }
}
