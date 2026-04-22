using Whycespace.Domain.OperationalSystem.Routing.Path;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Routing.Execution;

public sealed record ExecutionStartedEvent(
    ExecutionId ExecutionId,
    PathId PathId,
    Timestamp StartedAt) : DomainEvent;
