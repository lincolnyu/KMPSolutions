using System;
using System.Collections.Generic;
using System.Linq;

namespace KMPCoreObjects.Accounting
{
    public class Account
    {
        public Account(string name)
        {
            Name = name;
        }

        public Account? Parent { get; set; }
        public List<Account> Children { get; } = new List<Account>();

        public string Name { get; set; }

        public string FullName => Parent != null ? Parent.FullName + "." + Name : Name;

        public decimal Balance 
        { 
            get
            {
                if (Children.Count > 0)
                {
                    if (childrenChanged_)
                    {
                        balance_ = Children.Sum(x => x.Balance);
                    }
                }
                return balance_;
            }
            set
            {
                if (Children.Count > 0)
                {
                    throw new InvalidOperationException("Setting balance to an aggregate account is not allowed");
                }

                balance_ = value;
                
                for(var p = Parent; p != null; p = p.Parent)
                {
                    p.childrenChanged_ = true;
                }
            }
        }

        private decimal balance_ = 0;
        private bool childrenChanged_ = false;
    }
}
