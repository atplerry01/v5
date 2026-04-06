using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Jurisdiction;

public sealed class JurisdictionPolicyStatus : ValueObject
{
    public static readonly JurisdictionPolicyStatus Draft = new("Draft");
    public static readonly JurisdictionPolicyStatus Active = new("Active");
    public static readonly JurisdictionPolicyStatus Suspended = new("Suspended");
    public static readonly JurisdictionPolicyStatus Retired = new("Retired");

    public string Value { get; }
    private JurisdictionPolicyStatus(string value) => Value = value;

    public bool IsTerminal => this == Retired;

    public static bool IsValidTransition(JurisdictionPolicyStatus from, JurisdictionPolicyStatus to) =>
        (from, to) switch
        {
            _ when from == Draft && to == Active => true,
            _ when from == Active && to == Suspended => true,
            _ when from == Suspended && to == Active => true,
            _ when from == Active && to == Retired => true,
            _ when from == Suspended && to == Retired => true,
            _ => false
        };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
