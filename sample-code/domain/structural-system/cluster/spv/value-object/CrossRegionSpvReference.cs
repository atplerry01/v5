namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

/// <summary>
/// Reference to an SPV in another region. Immutable cross-region identifier.
/// </summary>
public sealed record CrossRegionSpvReference
{
    public Guid SpvId { get; }
    public string RegionId { get; }
    public string JurisdictionCode { get; }

    private CrossRegionSpvReference(Guid spvId, string regionId, string jurisdictionCode)
    {
        SpvId = spvId;
        RegionId = regionId;
        JurisdictionCode = jurisdictionCode;
    }

    public static CrossRegionSpvReference From(Guid spvId, string regionId, string jurisdictionCode) =>
        string.IsNullOrWhiteSpace(regionId)
            ? throw new ArgumentException("Region ID is required.")
            : new(spvId, regionId, jurisdictionCode);
}
