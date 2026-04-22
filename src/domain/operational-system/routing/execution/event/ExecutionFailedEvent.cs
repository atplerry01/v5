using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Routing.Execution;

public sealed record ExecutionFailedEvent(
    ExecutionId ExecutionId,
    string Reason,
    Timestamp FailedAt) : DomainEvent;
