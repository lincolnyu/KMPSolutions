using System;

namespace KMPAccounting.Objects.BookKeeping
{
    public abstract class Entry
    {
        public Entry(DateTime dateTime)
        {
            DateTime = dateTime;
        }

        // When the transaction occurs
        public DateTime DateTime { get; }

        public abstract void Redo();
        public abstract void Undo();
    }
}
