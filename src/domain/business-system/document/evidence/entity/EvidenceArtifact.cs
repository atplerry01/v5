namespace Whycespace.Domain.BusinessSystem.Document.Evidence;

public sealed class EvidenceArtifact
{
    public Guid ArtifactId { get; }
    public string ArtifactType { get; }

    public EvidenceArtifact(Guid artifactId, string artifactType)
    {
        if (artifactId == Guid.Empty)
            throw new ArgumentException("ArtifactId must not be empty.", nameof(artifactId));

        if (string.IsNullOrWhiteSpace(artifactType))
            throw new ArgumentException("ArtifactType must not be empty.", nameof(artifactType));

        ArtifactId = artifactId;
        ArtifactType = artifactType;
    }
}
