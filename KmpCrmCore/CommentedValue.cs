namespace KmpCrmCore
{
    public class CommentedValue<T>
    {
        public CommentedValue(T value, string comments="") 
        { 
            Value = value;
            Comments = comments;
        }

        public T Value { get; set; }
        public string Comments { get; set; }
    }
}
