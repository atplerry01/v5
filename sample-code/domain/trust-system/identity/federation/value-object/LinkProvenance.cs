namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Provenance of a federation link — how and why it was created.
/// Tracks the origin source and any supporting evidence reference.
/// </summary>
public sealed record LinkProvenance
{
    public ProvenanceSource Source { get; }
    public DateTimeOffset CreatedAt { get; }
    public string? EvidenceReference { get; }

    public LinkProvenance(ProvenanceSource source, DateTimeOffset createdAt, string? evidenceReference = null)
    {
        Source = source;
        CreatedAt = createdAt;
        EvidenceReference = evidenceReference;
    }

    public static LinkProvenance UserInitiated(DateTimeOffset at, string? evidence = null) =>
        new(ProvenanceSource.UserInitiated, at, evidence);

    public static LinkProvenance IssuerAsserted(DateTimeOffset at, string? evidence = null) =>
        new(ProvenanceSource.IssuerAsserted, at, evidence);

    public static LinkProvenance SystemInferred(DateTimeOffset at, string? evidence = null) =>
        new(ProvenanceSource.SystemInferred, at, evidence);
}

public enum ProvenanceSource
{
    UserInitiated,
    IssuerAsserted,
    SystemInferred
}
