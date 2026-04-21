using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Administration;

public readonly record struct AdministrationDescriptor
{
    public ClusterRef ClusterReference { get; }
    public string AdministrationName { get; }

    public AdministrationDescriptor(ClusterRef clusterReference, string administrationName)
    {
        Guard.Against(clusterReference == default, "AdministrationDescriptor cluster reference cannot be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(administrationName), "AdministrationDescriptor name must not be null or whitespace.");

        ClusterReference = clusterReference;
        AdministrationName = administrationName;
    }
}
