using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Jurisdiction;

public sealed class JurisdictionConflictException : DomainException
{
    public JurisdictionConflictException(string jurisdictionA, string jurisdictionB, string action)
        : base("JURISDICTION_CONFLICT", $"Conflicting overlay rules from '{jurisdictionA}' and '{jurisdictionB}' for action '{action}'.") { }
}

public sealed class JurisdictionPolicyRetiredException : DomainException
{
    public JurisdictionPolicyRetiredException()
        : base("JURISDICTION_POLICY_RETIRED", "This jurisdiction policy has been retired and cannot be modified.") { }
}
