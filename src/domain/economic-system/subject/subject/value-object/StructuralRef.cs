namespace Whycespace.Domain.EconomicSystem.Subject.Subject;

public enum StructuralRefType
{
    Cluster,
    Subcluster,
    Spv,
    Provider,
    Participant
}

public sealed record StructuralRef
{
    public StructuralRefType RefType { get; }
    public string RefId { get; }

    public StructuralRef(StructuralRefType refType, string refId)
    {
        if (string.IsNullOrWhiteSpace(refId))
            throw new ArgumentException("RefId cannot be empty", nameof(refId));

        RefType = refType;
        RefId = refId;
    }

    // Optional helpers to ease adoption (non-breaking)
    public static StructuralRef FromCluster(string id)     => new(StructuralRefType.Cluster, id);
    public static StructuralRef FromSubcluster(string id)  => new(StructuralRefType.Subcluster, id);
    public static StructuralRef FromSpv(string id)         => new(StructuralRefType.Spv, id);
    public static StructuralRef FromProvider(string id)    => new(StructuralRefType.Provider, id);
    public static StructuralRef FromParticipant(string id) => new(StructuralRefType.Participant, id);
}
