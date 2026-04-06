using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Vote;

public sealed record BallotCreatedEvent(Guid BallotId) : DomainEvent;
