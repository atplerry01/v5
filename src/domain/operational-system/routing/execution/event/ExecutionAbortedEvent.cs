using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Routing.Execution;

public sealed record ExecutionAbortedEvent(
    ExecutionId ExecutionId,
    string Reason,
    Timestamp AbortedAt) : DomainEvent;
