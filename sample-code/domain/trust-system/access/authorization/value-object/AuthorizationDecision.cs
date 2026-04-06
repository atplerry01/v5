namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public sealed record AuthorizationDecision(string Value)
{
    public static readonly AuthorizationDecision Pending = new("Pending");
    public static readonly AuthorizationDecision Approved = new("Approved");
    public static readonly AuthorizationDecision Denied = new("Denied");

    public bool IsTerminal => this == Approved || this == Denied;

    public override string ToString() => Value;
}
