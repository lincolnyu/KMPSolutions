namespace KMPBusinessRelationship.ImportExport
{
    public static class ExcelCommon
    {
        public static class ReferrersColumns
        {
            public const int Index = 1;
            public const int ProviderNumber = 2;
            public const int Name = 3;
            public const int Phone = 4;
            public const int Fax = 5;
            public const int PracticeName = 6;
            public const int Address = 7;
            public const int PostalAddress = 8;
            public const int Remarks = 9;

            public static IEnumerable<string> GetAllColumns()
            {
                yield return "Index";
                yield return "Provier Number";
                yield return "Name";
                yield return "Phone";
                yield return "Fax";
                yield return "PracticeName";
                yield return "Address";
                yield return "PostalAddress";
                yield return "Remarks";
            }
        }

        public static class VisitsColumns
        {
            public const int Index = 1;
            public const int CareNumber = 2;
            public const int GivenName = 3;
            public const int Surname = 4;
            public const int DateOfBirth = 5;
            public const int Gender = 6;
            public const int Phone = 7;
            public const int Address = 8;
            public const int ReferrerID = 9;
            public const int ReferralDate = 10;
            public const int Visit = 11;
            public const int IfClaimed = 12;
            public const int Remarks = 13;

            public static IEnumerable<string> GetAllColumns()
            {
                yield return "Index";
                yield return "Care Number";
                yield return "Given Name";
                yield return "Surname";
                yield return "Date Of Birth";
                yield return "Gender";
                yield return "Phone";
                yield return "Address";
                yield return "Referrer ID";
                yield return "Referral Date";
                yield return "Visit";
                yield return "If Claimed";
                yield return "Remarks";
            }
        }
    }
}
