using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyPackage;

public sealed record PolicyPackageDeployedEvent(
    PolicyPackageId Id,
    PackageVersion Version) : DomainEvent;
