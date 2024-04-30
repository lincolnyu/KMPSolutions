using System;
using System.Collections.Generic;

namespace KMPBusinessRelationship.Objects
{
    public class Referrer : Person, IEquatable<Referrer>
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

        public override string Id => ProviderNumber;

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
                if (!string.IsNullOrEmpty(value))
                {
                    var valueSplit = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var v in valueSplit)
                    {
                        OtherProviderNumbers.Add(v.Trim());
                    }
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

        public bool Equals(Referrer other)
        {
            if (ProviderNumber !=  other.ProviderNumber) return false;
            if (OtherProviderNumbers.Count != other.OtherProviderNumbers.Count) return false;
            foreach (var opn in other.OtherProviderNumbers)
            {
                if (!OtherProviderNumbers.Contains(opn)) return false;
            }
            if (Name != other.Name) return false;
            if (Phone != other.Phone) return false;
            if (Fax != other.Fax) return false;
            if (PracticeName != other.PracticeName) return false;
            if (Address != other.Address) return false;
            if (PostalAddress != other.PostalAddress) return false;
            if (Remarks != other.Remarks) return false;
            return true;
        }
    }
}
