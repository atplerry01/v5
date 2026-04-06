namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Status of a federated identity. Explicit transitions only.
/// </summary>
public sealed record FederationStatus
{
    public string Value { get; }

    private FederationStatus(string value) => Value = value;

    public static readonly FederationStatus Active = new("Active");
    public static readonly FederationStatus Suspended = new("Suspended");
    public static readonly FederationStatus Revoked = new("Revoked");

    public bool IsTerminal => this == Revoked;

    public static bool IsValidTransition(FederationStatus from, FederationStatus to) =>
        (from, to) switch
        {
            _ when from == Active && to == Suspended => true,
            _ when from == Active && to == Revoked => true,
            _ when from == Suspended && to == Active => true,
            _ when from == Suspended && to == Revoked => true,
            _ => false
        };

    public override string ToString() => Value;
}
