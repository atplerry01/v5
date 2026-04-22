using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.SystemVerification;

public sealed record SystemVerificationInitiatedEvent(
    SystemVerificationId Id,
    string TargetSystem,
    DateTimeOffset InitiatedAt) : DomainEvent;
