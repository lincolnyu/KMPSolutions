using System;

namespace KMPAccounting.Objects.Fundamental
{
    public class WeakPointable<T> : IDisposable where T : WeakPointable<T>
    {
        ~WeakPointable()
        {
            Dispose(false);
        }

        public virtual void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (weakHandle_ != null)
            {
                weakHandle_.Target = null;  // This invalidate the handle
                weakHandle_ = null;
            }
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        internal void SetWeakHandle(WeakPointer<T>.Handle handle)
        {
            weakHandle_ = handle;
        }

        public WeakPointer<T>.Handle? GetWeakHandle()
        {
            return weakHandle_;
        }

        private WeakPointer<T>.Handle? weakHandle_;
    }
}
