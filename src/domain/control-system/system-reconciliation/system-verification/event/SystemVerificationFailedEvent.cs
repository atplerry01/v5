using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.SystemVerification;

public sealed record SystemVerificationFailedEvent(
    SystemVerificationId Id,
    string FailureReason,
    DateTimeOffset FailedAt) : DomainEvent;
