using System.Collections.Generic;
using System.ComponentModel;

namespace KMPBookingCore
{
    public class DbObject: INotifyPropertyChanged
    {
        public enum DbState
        {
            Synced,
            New,
            Dirty,
        }

        public DbState ObjDbState { get; protected set; }

        protected DbObject() 
        {
            if (LoadingFromDb)
            {
                ObjDbState = DbState.Synced;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static bool LoadingFromDb { get; private set; }
        private static Stack<bool> _loadingFromDbStack = new Stack<bool>();

        public static void PushLoadingFromDb(bool value)
        {
            _loadingFromDbStack.Push(LoadingFromDb);
            LoadingFromDb = value;
        }

        public static void PopLoadingFromDb()
        {
            LoadingFromDb = _loadingFromDbStack.Pop();
        }

        protected void RaiseEventChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            if (!LoadingFromDb)
            {
                if (ObjDbState == DbState.Synced)
                {
                    ObjDbState = DbState.Dirty;
                }
            }
        }
    }
}
