using System;
using System.Collections.Generic;
using System.Text;

namespace KMPBookingCore
{
    public static class UiUtils
    {
        public class AutoResetSuppressor
        {
            public bool Suppressing { get; private set; } = false;

            public void Run(Action proc)
            {
                if (!Suppressing)
                {
                    Suppressing = true;
                    proc();
                    Suppressing = false;
                }
            }
        }
    }
}
