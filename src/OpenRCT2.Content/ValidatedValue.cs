namespace OpenRCT2.Content
{
    public abstract class ValidatedValue
    {
        public bool? IsValid { get; set; }
        public string Message { get; set; }

        public virtual void ResetValidation()
        {
            IsValid = null;
            Message = null;
        }
    }

    public class ValidatedValue<T> : ValidatedValue
    {
        public T Value { get; set; }
    }
}
