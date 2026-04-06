namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

public sealed record ServiceScope(string Value)
{
    public override string ToString() => Value;
}
