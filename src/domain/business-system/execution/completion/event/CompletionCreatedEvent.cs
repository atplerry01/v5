namespace Whycespace.Domain.BusinessSystem.Execution.Completion;

public sealed record CompletionCreatedEvent(CompletionId CompletionId, CompletionTargetId TargetId);
