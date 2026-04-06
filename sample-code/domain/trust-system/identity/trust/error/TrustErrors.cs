namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public static class TrustErrors
{
    public static DomainException NotFound(Guid trustProfileId)
        => new("TRUST_NOT_FOUND", $"Trust profile '{trustProfileId}' was not found.");

    public static DomainException Frozen(Guid trustProfileId)
        => new("TRUST_FROZEN", $"Trust profile '{trustProfileId}' is frozen.");
}
