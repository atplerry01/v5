using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyPackage;

public sealed record PolicyPackageRetiredEvent(
    PolicyPackageId Id) : DomainEvent;
