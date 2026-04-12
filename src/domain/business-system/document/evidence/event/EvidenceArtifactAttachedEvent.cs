namespace Whycespace.Domain.BusinessSystem.Document.Evidence;

public sealed record EvidenceArtifactAttachedEvent(EvidenceId EvidenceId, Guid ArtifactId, string ArtifactType);
