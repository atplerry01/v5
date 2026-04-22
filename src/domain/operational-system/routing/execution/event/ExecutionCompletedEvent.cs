using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Routing.Execution;

public sealed record ExecutionCompletedEvent(
    ExecutionId ExecutionId,
    Timestamp CompletedAt) : DomainEvent;
