using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Transition;

public sealed record TransitionExecutedEvent(Guid TransitionId) : DomainEvent;
