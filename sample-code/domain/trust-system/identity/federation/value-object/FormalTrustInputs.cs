namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Credential validity rate — ratio of valid credentials to total.
/// Bounded [0, 1]. Validated. Deterministic.
/// </summary>
public sealed record CredentialValidityRate
{
    public decimal Value { get; }

    public CredentialValidityRate(int validCredentials, int totalCredentials)
    {
        if (totalCredentials < 0)
            throw new ArgumentOutOfRangeException(nameof(totalCredentials), "Cannot be negative.");
        if (validCredentials < 0)
            throw new ArgumentOutOfRangeException(nameof(validCredentials), "Cannot be negative.");
        if (validCredentials > totalCredentials)
            throw new ArgumentException("Valid credentials cannot exceed total credentials.");

        Value = totalCredentials == 0 ? 1m : (decimal)validCredentials / totalCredentials;
    }

    public static CredentialValidityRate Perfect => new(1, 1);
    public override string ToString() => $"{Value:P1}";
}

/// <summary>
/// Revocation rate — ratio of revoked credentials to total.
/// Bounded [0, 1]. Validated. Deterministic.
/// </summary>
public sealed record RevocationRate
{
    public decimal Value { get; }

    public RevocationRate(int revokedCredentials, int totalCredentials)
    {
        if (totalCredentials < 0)
            throw new ArgumentOutOfRangeException(nameof(totalCredentials), "Cannot be negative.");
        if (revokedCredentials < 0)
            throw new ArgumentOutOfRangeException(nameof(revokedCredentials), "Cannot be negative.");
        if (revokedCredentials > totalCredentials)
            throw new ArgumentException("Revoked credentials cannot exceed total credentials.");

        Value = totalCredentials == 0 ? 0m : (decimal)revokedCredentials / totalCredentials;
    }

    public static RevocationRate None => new(0, 1);
    public override string ToString() => $"{Value:P1}";
}

/// <summary>
/// Incident rate — incidents per active link.
/// Bounded [0, ∞) but normalized for scoring. Validated. Deterministic.
/// </summary>
public sealed record IncidentRate
{
    public decimal Value { get; }
    public int IncidentCount { get; }
    public int ActiveLinkCount { get; }

    public IncidentRate(int incidentCount, int activeLinkCount)
    {
        if (incidentCount < 0)
            throw new ArgumentOutOfRangeException(nameof(incidentCount), "Cannot be negative.");
        if (activeLinkCount < 0)
            throw new ArgumentOutOfRangeException(nameof(activeLinkCount), "Cannot be negative.");

        IncidentCount = incidentCount;
        ActiveLinkCount = activeLinkCount;
        Value = activeLinkCount == 0 ? (incidentCount > 0 ? 1m : 0m) : (decimal)incidentCount / activeLinkCount;
    }

    public static IncidentRate None => new(0, 1);
    public override string ToString() => $"{Value:F2} ({IncidentCount} incidents / {ActiveLinkCount} links)";
}
