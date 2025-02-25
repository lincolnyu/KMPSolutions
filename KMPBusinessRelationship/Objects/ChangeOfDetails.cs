﻿using KMPCommon;

namespace KMPBusinessRelationship.Objects
{
    public class ChangeOfDetails<TPerson> : Event<ChangeOfDetails<TPerson>> where TPerson : Person
    {
        public TPerson Person { get; set; }
        public string PropertyName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public void SetCurrentValueToOld()
        {
            OldValue = GetCurrentValueAsString();
        }

        public void SetCurrentValueToNew()
        {
            NewValue = GetCurrentValueAsString();
        }

        private string GetCurrentValueAsString()
        {
            var property = Person.GetType().GetProperty(PropertyName);
            var val = property.GetValue(Person, null);
            return val.ToString();
        }

        public void SetOldValue(object oldValue)
        {
            OldValue = oldValue.ToString();
        }

        public void SetNewValue(object newValue) 
        { 
            NewValue = newValue.ToString();
        }

        public override void Redo()
        {
            var property = Person.GetType().GetProperty(PropertyName);
            var propertyType = property.PropertyType;
            var newValueObj = propertyType.Parse(NewValue!);
            property.SetValue(Person, newValueObj);
        }

        public override void Undo()
        {
            var property = Person.GetType().GetProperty(PropertyName);
            var propertyType = property.PropertyType;
            var oldValueObj = propertyType.Parse(OldValue!);
            property.SetValue(Person, oldValueObj);
        }

        public override bool Equals(ChangeOfDetails<TPerson> other)
        {
            if (Person.Id != other.Person.Id) return false;
            if (PropertyName != other.PropertyName) return false;
            if (OldValue != other.OldValue) return false;
            if (NewValue != other.NewValue) return false;
            return true;
        }
    }
}
