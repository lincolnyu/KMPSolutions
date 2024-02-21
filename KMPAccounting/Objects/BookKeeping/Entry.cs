using System;

namespace KMPAccounting.Objects.BookKeeping
{
    public abstract class Entry : IEquatable<Entry>
    {
        public Entry(DateTime dateTime)
        {
            DateTime = dateTime;
        }

        // When the transaction occurs
        public DateTime DateTime { get; }

        public abstract bool Equals(Entry other);

        public abstract void Redo();
        public abstract void Undo();

        public abstract string SerializeToLine();
    }
}
