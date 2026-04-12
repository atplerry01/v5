namespace Whycespace.Domain.BusinessSystem.Document.Retention;

public sealed class RetentionDomainException : Exception
{
    public RetentionDomainException(string message) : base(message) { }
}
