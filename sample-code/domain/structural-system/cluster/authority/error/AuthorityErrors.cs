namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public sealed class AuthorityException : DomainException
{
    public AuthorityException(string message) : base("AUTHORITY_ERROR", message) { }
}
