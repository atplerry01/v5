namespace Whycespace.Domain.BusinessSystem.Execution.Completion;

public sealed record CompletionFailedEvent(CompletionId CompletionId, string Reason);
