﻿using System;
using System.Collections.Generic;

namespace KmpCrmCore
{
    using DateOnly = DateTime;

    public class Customer
    {
        public string MedicareNumber { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public CommentedValue<bool> InitialLetter { get; set; }
        public List<CommentedValue<VisitBatch>> VisitBatches { get; private set; } = new List<CommentedValue<VisitBatch>>();
        public Gp ReferingGP { get; set; }
        public DateOnly? ReferringDate { get; set; }
        public string LegacyData { get; set; }
    }
}