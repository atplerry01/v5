namespace Whycespace.Shared.Contracts.Economic.Routing.Execution;

public sealed record GetExecutionByIdQuery(Guid ExecutionId);

public sealed record ListExecutionsByPathIdQuery(Guid PathId);

public sealed record ListExecutionsByStatusQuery(string Status);
