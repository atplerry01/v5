using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public sealed class TrustHistory : Entity
{
    public Guid TrustProfileId { get; private set; }
    public TrustFactor Factor { get; private set; } = null!;
    public decimal Weight { get; private set; }
    public decimal ScoreBefore { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }

    private TrustHistory() { }

    public static TrustHistory Record(Guid trustProfileId, TrustFactor factor, decimal weight, decimal scoreBefore, DateTimeOffset timestamp)
    {
        return new TrustHistory
        {
            Id = DeterministicIdHelper.FromSeed($"TrustHistory:{trustProfileId}:{factor.Value}:{scoreBefore}"),
            TrustProfileId = trustProfileId,
            Factor = factor,
            Weight = weight,
            ScoreBefore = scoreBefore,
            RecordedAt = timestamp
        };
    }
}
