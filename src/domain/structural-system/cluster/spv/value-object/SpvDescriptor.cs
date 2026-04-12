namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public readonly record struct SpvDescriptor
{
    public Guid ClusterReference { get; }
    public string SpvName { get; }
    public SpvType SpvType { get; }

    public SpvDescriptor(Guid clusterReference, string spvName, SpvType spvType)
    {
        if (clusterReference == Guid.Empty)
            throw SpvErrors.MissingDescriptor();

        if (string.IsNullOrWhiteSpace(spvName))
            throw SpvErrors.MissingDescriptor();

        if (!Enum.IsDefined(spvType))
            throw SpvErrors.MissingDescriptor();

        ClusterReference = clusterReference;
        SpvName = spvName;
        SpvType = spvType;
    }
}
