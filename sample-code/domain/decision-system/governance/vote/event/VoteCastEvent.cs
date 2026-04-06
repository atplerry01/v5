using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Vote;

public sealed record VoteCastEvent(Guid VoteId, Guid VoterIdentityId) : DomainEvent;
