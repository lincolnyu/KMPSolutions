using System;

namespace KMPBookingCore.UserInterface
{
    public class StackBottomActioner
    {
        public StackBottomActioner(Action finalAction)
        {
            _finalAction = finalAction;
        }
        public class Guard : IDisposable
        {
            public Guard(StackBottomActioner checker)
            {
                _actioner = checker;
                _actioner._stackDepth++;
            }

            public void Dispose()
            {
                if (_actioner != null)
                {
                    _actioner._stackDepth--;
                    if (_actioner._stackDepth == 0)
                    {
                        _actioner._finalAction?.Invoke();
                    }
                    _actioner = null;
                }
            }

            private StackBottomActioner _actioner;
        }
        private int _stackDepth;
        private Action _finalAction;
    }
}
