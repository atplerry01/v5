namespace Whycespace.Domain.EconomicSystem.Subject.Subject;

public sealed record EconomicRef
{
    public EconomicRefType RefType { get; }
    public string RefId { get; }

    public EconomicRef(EconomicRefType refType, string refId)
    {
        if (string.IsNullOrWhiteSpace(refId))
            throw new ArgumentException("RefId cannot be empty", nameof(refId));

        RefType = refType;
        RefId = refId;
    }
}

public enum EconomicRefType
{
    VaultAccount,
    CapitalAccount
}

// Canonical destination rules. NOT enforced in EconomicRef constructor
// to avoid breaking changes — composition/runtime calls Validate() as
// an explicit gate when wiring a subject to its economic reference.
public static class EconomicRefRules
{
    public static void Validate(SubjectType subjectType, EconomicRefType refType)
    {
        var valid = subjectType switch
        {
            SubjectType.Participant => refType == EconomicRefType.CapitalAccount,
            SubjectType.Provider    => refType == EconomicRefType.CapitalAccount,
            SubjectType.SPV         => refType == EconomicRefType.VaultAccount,
            SubjectType.CWG         => refType == EconomicRefType.VaultAccount,
            SubjectType.Cluster     => refType == EconomicRefType.VaultAccount,
            _ => false
        };

        if (!valid)
            throw new InvalidOperationException(
                $"Invalid EconomicRefType '{refType}' for SubjectType '{subjectType}'.");
    }
}
