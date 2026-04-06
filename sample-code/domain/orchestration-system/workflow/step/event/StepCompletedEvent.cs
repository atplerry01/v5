using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Step;

public sealed record StepCompletedEvent(Guid StepId) : DomainEvent;
