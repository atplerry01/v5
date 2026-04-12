namespace Whycespace.Domain.BusinessSystem.Entitlement.Right;

public sealed class RightDefinition
{
    public RightScopeId ScopeId { get; }
    public string Capability { get; }
    public string Constraints { get; }

    public RightDefinition(RightScopeId scopeId, string capability, string constraints)
    {
        if (scopeId == default)
            throw new ArgumentException("ScopeId must not be empty.", nameof(scopeId));

        if (string.IsNullOrWhiteSpace(capability))
            throw new ArgumentException("Capability must not be empty.", nameof(capability));

        if (string.IsNullOrWhiteSpace(constraints))
            throw new ArgumentException("Constraints must not be empty.", nameof(constraints));

        ScopeId = scopeId;
        Capability = capability;
        Constraints = constraints;
    }
}
