using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

public readonly record struct IdentityGraphDescriptor
{
    public Guid PrimaryIdentityReference { get; }
    public string GraphContext { get; }

    public IdentityGraphDescriptor(Guid primaryIdentityReference, string graphContext)
    {
        Guard.Against(primaryIdentityReference == Guid.Empty, "IdentityGraphDescriptor.PrimaryIdentityReference must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(graphContext), "IdentityGraphDescriptor.GraphContext must not be empty.");

        PrimaryIdentityReference = primaryIdentityReference;
        GraphContext = graphContext.Trim();
    }
}
