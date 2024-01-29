namespace KMPAccounting.Objects.Fundamental
{
    public class WeakPointer<T> where T : WeakPointable<T>
    {
        public class Handle
        {
            public T? Target;
            public int RefCount = 0;
        }

        public WeakPointer(T target)
        {
            var existingHandle = target.GetWeakHandle();
            if (existingHandle != null)
            {
                handle_ = existingHandle;
            }
            else
            {
                handle_ = new Handle { Target = target, RefCount = 1 };
                target.SetWeakHandle(handle_);
            }
        }

        public bool TryGetTarget(out T? target)
        {
            if (handle_.Target != null)
            {
                target = handle_.Target;
                return true;
            }
            target = default;
            return false;
        }

        private Handle handle_;
    }
}
