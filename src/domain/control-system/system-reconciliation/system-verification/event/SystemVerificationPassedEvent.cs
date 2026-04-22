using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.SystemVerification;

public sealed record SystemVerificationPassedEvent(
    SystemVerificationId Id,
    DateTimeOffset PassedAt) : DomainEvent;
