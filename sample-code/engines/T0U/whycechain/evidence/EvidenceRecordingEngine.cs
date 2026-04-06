using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T0U.WhyceChain.Evidence;

public sealed class EvidenceRecordingEngine : ChainEngineBase
{
    private readonly IClock _clock;

    public EvidenceRecordingEngine(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public ChainResult Record(RecordEvidenceCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return new ChainResult(true, string.Empty, _clock.UtcNowOffset);
    }
}
