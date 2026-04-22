using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Contract;

public sealed record SubscriberConstraint
{
    public DomainRoute SubscriberRoute { get; }
    public int MinSchemaVersion { get; }
    public ContractCompatibilityMode RequiredCompatibilityMode { get; }

    public SubscriberConstraint(
        DomainRoute subscriberRoute,
        int minSchemaVersion,
        ContractCompatibilityMode requiredCompatibilityMode)
    {
        Guard.Against(!subscriberRoute.IsValid(), "SubscriberConstraint requires a valid SubscriberRoute.");
        Guard.Against(minSchemaVersion < 1, "SubscriberConstraint MinSchemaVersion must be ≥ 1.");

        SubscriberRoute = subscriberRoute;
        MinSchemaVersion = minSchemaVersion;
        RequiredCompatibilityMode = requiredCompatibilityMode;
    }
}
