using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Vote;

public sealed record BallotClosedEvent(Guid BallotId) : DomainEvent;
