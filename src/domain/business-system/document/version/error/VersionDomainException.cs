namespace Whycespace.Domain.BusinessSystem.Document.Version;

public sealed class VersionDomainException : Exception
{
    public VersionDomainException(string message) : base(message) { }
}
