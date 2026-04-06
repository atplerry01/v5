using Whycespace.Domain.SharedKernel;
using Whycespace.Domain.SharedKernel.Primitive.Identity;

namespace Whycespace.Domain.DecisionSystem.Governance.Vote;

public class BallotAggregate : AggregateRoot
{
    public IdentityId InitiatorIdentityId { get; private set; } = default!;
    public BallotStatus Status { get; private set; } = BallotStatus.Open;
    private readonly List<Guid> _voterIds = [];
    public IReadOnlyList<Guid> VoterIds => _voterIds.AsReadOnly();

    protected BallotAggregate() { }

    public static BallotAggregate Create(Guid ballotId, IdentityId initiatorIdentityId)
    {
        Guard.AgainstDefault(ballotId);
        Guard.AgainstNull(initiatorIdentityId);

        var ballot = new BallotAggregate
        {
            Id = ballotId,
            InitiatorIdentityId = initiatorIdentityId,
            Status = BallotStatus.Open
        };

        return ballot;
    }

    public void CastVote(Guid voteId, IdentityId voterIdentityId)
    {
        Guard.AgainstDefault(voteId);
        Guard.AgainstNull(voterIdentityId);

        EnsureInvariant(
            Status == BallotStatus.Open,
            "BALLOT_NOT_OPEN",
            $"Cannot cast vote on ballot in '{Status}' status.");

        EnsureInvariant(
            !_voterIds.Contains(voterIdentityId.Value),
            "DUPLICATE_VOTE",
            "Voter has already cast a vote on this ballot.");

        _voterIds.Add(voterIdentityId.Value);
        RaiseDomainEvent(new VoteCastEvent(voteId, voterIdentityId.Value));
    }

    public void Close()
    {
        EnsureInvariant(
            Status == BallotStatus.Open,
            "ALREADY_IN_STATE",
            "Ballot is already closed.");

        Status = BallotStatus.Closed;
        RaiseDomainEvent(new BallotClosedEvent(Id));
    }
}
