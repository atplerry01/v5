using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T0U.WhyceChain.Anchoring;

public sealed class MerkleAnchoringEngine : ChainEngineBase
{
    private readonly IClock _clock;

    public MerkleAnchoringEngine(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public ChainResult Anchor(AnchorMerkleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return new ChainResult(true, string.Empty, _clock.UtcNowOffset);
    }
}
