﻿using KMPBusinessRelationship;
using KMPBusinessRelationship.Objects;

namespace KMPBusinessRelationshipTest
{
    internal static class Utility
    {
        public static void CreateSampleGPs(this Repository repo)
        {
            var gp1 = new GeneralPractitioner
            {
                ProviderNumber = "ABCD1234",
                Name = "Tan, Lily",
                Address = "31 Smith Road, Weetangera ACT, 2614",
                Phone = "34561234",
                Fax = "34561235"
            };

            {
                var res = repo.SearchOrAddGeneralPractitioner(gp1, out var gp1InRepo);

                Assert.That(res, Is.EqualTo(false));
                Assert.That(gp1InRepo, Is.SameAs(gp1));
            }

            {
                var gp1Dup = new GeneralPractitioner
                {
                    ProviderNumber = "ABCD1234",
                };
                var res = repo.SearchOrAddGeneralPractitioner(gp1Dup, out var gp1InRepo);
                Assert.That(res, Is.EqualTo(true));
                Assert.That(gp1InRepo, Is.SameAs(gp1));
            }

            var gp2 = new GeneralPractitioner
            {
                ProviderNumber = "BCDE2345",
                Name = "James, Roger",
                Address = "12 Kent Road, Philip ACT, 2605",
                Phone = "12567834",
                Fax = "12567836"
            };

            {
                var res = repo.SearchOrAddGeneralPractitioner(gp2, out var gp2InRepo);

                Assert.That(res, Is.EqualTo(false));
                Assert.That(gp2InRepo, Is.SameAs(gp2));
            }
        }

        public static void CreateSampleClients(this Repository repo)
        {
            var client1 = new Client
            {
                MedicareNumber = "ABCD1234",
                Name = "Wood, Thomas",
                DateOfBirth = new DateTime(1950, 1, 3),
                PhoneNumber = "45673421",
            };

            {
                var res = repo.SearchOrAddClient(client1, out var client1InRepo);

                Assert.That(res, Is.EqualTo(false));
                Assert.That(client1InRepo, Is.SameAs(client1InRepo));
            }

            {
                var client1Dup1 = new Client
                {
                    MedicareNumber = "ABCD1234",
                };
                var res = repo.SearchOrAddClient(client1Dup1, out var client1InRepo);
                Assert.That(res, Is.EqualTo(true));
                Assert.That(client1InRepo, Is.SameAs(client1));
            }

            {
                var client1Dup2 = new Client
                {
                    Name = "Wood, Thomas",
                    DateOfBirth = new DateTime(1950, 1, 3),
                    PhoneNumber = "432061291"
                };
                var res = repo.SearchOrAddClient(client1Dup2, out var client1InRepo);
                Assert.That(res, Is.EqualTo(true));
                Assert.That(client1InRepo, Is.SameAs(client1));
            }

            var client2 = new Client
            {
                MedicareNumber = "DEFG1234",
                Name = "Wuu, Emily",
                DateOfBirth = new DateTime(1973, 1, 3),
                PhoneNumber = "45673421",
            };

            {
                var res = repo.SearchOrAddClient(client2, out var client2InRepo);

                Assert.That(res, Is.EqualTo(false));
                Assert.That(client2InRepo, Is.SameAs(client2));
            }
        }
    }
}
