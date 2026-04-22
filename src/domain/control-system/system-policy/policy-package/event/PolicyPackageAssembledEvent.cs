using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyPackage;

public sealed record PolicyPackageAssembledEvent(
    PolicyPackageId Id,
    string Name,
    PackageVersion Version,
    IReadOnlySet<string> PolicyDefinitionIds) : DomainEvent;
