namespace Whycespace.Domain.BusinessSystem.Execution.Activation;

public sealed record ActivationCreatedEvent(ActivationId ActivationId, ActivationTargetId TargetId);
