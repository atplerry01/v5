using Whycespace.Shared.Contracts.Infrastructure.Chain;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.EconomicSystem.Shared;

/// <summary>
/// phase5-operational-activation + phase6-hardening: real-Postgres chain
/// anchor with an <see cref="InMemoryChainAnchor"/> observer attached.
///
/// Primary durability path: <see cref="Whycespace.Platform.Host.Adapters.WhyceChainPostgresAdapter"/>.
/// If the real anchor throws, the mirror is NOT updated — the test's
/// block-count inspection must reflect real durability.
/// </summary>
internal sealed class MirroredChainAnchor : IChainAnchor
{
    private readonly IChainAnchor _real;
    private readonly InMemoryChainAnchor _mirror;

    public MirroredChainAnchor(IChainAnchor real, InMemoryChainAnchor mirror)
    {
        ArgumentNullException.ThrowIfNull(real);
        ArgumentNullException.ThrowIfNull(mirror);
        _real = real;
        _mirror = mirror;
    }

    public InMemoryChainAnchor Mirror => _mirror;

    public async Task<ChainBlock> AnchorAsync(Guid correlationId, IReadOnlyList<object> events, string decisionHash, CancellationToken cancellationToken = default)
    {
        var realBlock = await _real.AnchorAsync(correlationId, events, decisionHash, cancellationToken);
        await _mirror.AnchorAsync(correlationId, events, decisionHash, cancellationToken);
        return realBlock;
    }
}
