namespace Whycespace.Domain.ConstitutionalSystem.Chain.AnchorRecord;

public static class AnchorNotSealedSpecification
{
    public static bool IsSatisfiedBy(AnchorRecordStatus status) =>
        status != AnchorRecordStatus.Sealed;
}
