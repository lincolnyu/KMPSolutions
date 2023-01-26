using KmpCrmCore;
using System;
using System.IO;

namespace KmpCrmUwp
{
    internal class CrmData
    {
        public static CrmData Instance { get; private set; }

        public static bool Initialized => Instance != null;

        private CrmRepository _crmRepo;
        public CrmRepository CrmRepo
        {
            get
            {
                return _crmRepo;
            }
        }

        private CrmData(StreamReader sr)
        {
            var ser = new CsvSerializer();
            _crmRepo = ser.Deserialize(sr);
        }

        public static void Initialize(StreamReader sr)
        {
            if (Initialized)
            {
                throw new InvalidOperationException("CrmData already initialized.");
            }
            Instance = new CrmData(sr);
        }
    }
}
