using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Routing.Execution;

public sealed record ExecutionCompletedEvent(
    ExecutionId ExecutionId,
    Timestamp CompletedAt) : DomainEvent;
