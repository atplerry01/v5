namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

public static class IntelligenceErrors
{
    public static DomainException IdentityNotFound(string identityId) =>
        new("INTELLIGENCE_IDENTITY_NOT_FOUND", $"Intelligence profile not found for identity '{identityId}'.");

    public static DomainException InsufficientData(string identityId) =>
        new("INTELLIGENCE_INSUFFICIENT_DATA", $"Insufficient data to compute intelligence for identity '{identityId}'.");

    public static DomainException GraphCycleDetected(string nodeId) =>
        new("INTELLIGENCE_GRAPH_CYCLE", $"Graph cycle detected involving node '{nodeId}'.");
}
