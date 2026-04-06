namespace Whycespace.Domain.TrustSystem.Access.Session;

public sealed record SessionStatus(string Value)
{
    public static readonly SessionStatus Active = new("Active");
    public static readonly SessionStatus Expired = new("Expired");
    public static readonly SessionStatus Revoked = new("Revoked");

    public bool IsTerminal => this == Expired || this == Revoked;

    public override string ToString() => Value;
}
