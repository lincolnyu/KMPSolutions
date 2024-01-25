using KMPAccounting.Objects.Accounts;
using System;

namespace KMPAccounting.Objects.BookKeeping
{
    public class OpenAccount : Entry
    {
        public OpenAccount(DateTime dateTime, (AccountNodeReference, AccountNode.SideEnum)? parentAndSide, string name)
            : base(dateTime)
        {
            ParentAndSide = parentAndSide;
            Name = name;
        }

        public (AccountNodeReference, AccountNode.SideEnum)? ParentAndSide { get; }
        public AccountNode.SideEnum? Side { get; }
        public string Name { get; }

        public override void Redo()
        {
            if (ParentAndSide != null)
            {
                var (parent, side) = ParentAndSide.Value;
                var parentNode = parent.Get()!;
                parentNode.Children.Add(Name, new AccountNode(side, Name) { Parent = parentNode });
            }
            else
            {
                // Create the state.
                var state = new AccountsState(Name);
                AccountsState.AddState(Name, state);
            }
        }

        public override void Undo()
        {
            if (ParentAndSide != null)
            {
                var (parent, side) = ParentAndSide.Value;
                var parentNode = parent.Get()!;
                if (parentNode.Children.Remove(Name, out var child))
                {
                    child.Dispose();
                }
            }
            else
            {
                // Remove the state.
                AccountsState.RemoveState(Name);
            }
        }
    }
}
