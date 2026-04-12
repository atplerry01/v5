namespace Whycespace.Domain.BusinessSystem.Execution.Stage;

public sealed record StageCreatedEvent(StageId StageId, StageContextId ContextId);
