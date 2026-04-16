namespace Whycespace.Domain.EconomicSystem.Subject.Subject;

public static class EconomicRefMatchesSubjectSpecification
{
    public static bool IsSatisfiedBy(SubjectType subjectType, EconomicRefType refType) =>
        subjectType switch
        {
            SubjectType.Participant => refType == EconomicRefType.CapitalAccount,
            SubjectType.Provider    => refType == EconomicRefType.CapitalAccount,
            SubjectType.SPV         => refType == EconomicRefType.VaultAccount,
            SubjectType.CWG         => refType == EconomicRefType.VaultAccount,
            SubjectType.Cluster     => refType == EconomicRefType.VaultAccount,
            _ => false
        };
}
