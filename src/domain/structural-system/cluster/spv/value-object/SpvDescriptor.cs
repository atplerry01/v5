using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.StructuralSystem.Structure.ReferenceVocabularies;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public readonly record struct SpvDescriptor
{
    public ClusterRef ClusterReference { get; }
    public string SpvName { get; }
    public SpvType SpvType { get; }

    public SpvDescriptor(ClusterRef clusterReference, string spvName, SpvType spvType)
    {
        Guard.Against(clusterReference == default, "SpvDescriptor cluster reference cannot be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(spvName), "SpvDescriptor name must not be null or whitespace.");
        Guard.Against(!Enum.IsDefined(spvType), "SpvDescriptor SpvType must be a defined enum value.");

        ClusterReference = clusterReference;
        SpvName = spvName;
        SpvType = spvType;
    }
}
