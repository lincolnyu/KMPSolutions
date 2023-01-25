using KmpCrmCore;
using System.IO;

namespace KmpCrmUwp
{
    internal class CrmData
    {
        public static CrmData Instance { get; } = new CrmData();

        public CrmRepository _crmRepo;
        public CrmRepository CrmRepo
        {
            get
            {
                LoadIfNot();
                return _crmRepo;
            }
        }

        public CrmData()
        {
        }

        private void LoadIfNot()
        {
            if (_crmRepo == null)
            {
                var ser = new CsvSerializer();
                var sr = new StreamReader("C:\\Users\\quanb\\OneDrive\\temp\\kmptest");
                _crmRepo = ser.Deserialize(sr);
            }
        }
    }
}
