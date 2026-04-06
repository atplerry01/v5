namespace Whycespace.Domain.TrustSystem.Access.Request;

public sealed record AccessRequestStatus(string Value)
{
    public static readonly AccessRequestStatus Pending = new("Pending");
    public static readonly AccessRequestStatus Approved = new("Approved");
    public static readonly AccessRequestStatus Rejected = new("Rejected");

    public bool IsTerminal => this == Approved || this == Rejected;

    public override string ToString() => Value;
}
