namespace Whycespace.Platform.Api.Trust.Identity.Federation;

public sealed record RegisterIssuerDto(string Name, string IssuerType, decimal InitialTrust);
public sealed record LinkIdentityDto(string ExternalId, string IssuerId, decimal Confidence, int VerificationLevel, string ProvenanceSource, string? EvidenceReference = null);
public sealed record FederationLinksResponse(string IdentityId, IReadOnlyList<FederationLinkResponse> Links);
public sealed record FederationLinkResponse(string ExternalId, string IssuerId, decimal Confidence, int VerificationLevel, string Status, string ProvenanceSource, DateTimeOffset LinkedAt);
