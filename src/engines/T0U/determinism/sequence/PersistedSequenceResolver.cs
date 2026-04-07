using Whyce.Shared.Contracts.Infrastructure.Persistence;

namespace Whyce.Engines.T0U.Determinism.Sequence;

/// <summary>
/// HSID v2.1 sequence resolver backed by <see cref="ISequenceStore"/>. The
/// store provides the atomic, distributed counter; this class wraps it into
/// the locked X3 width by modulo (0..0xFFF). Wrap is acceptable because the
/// SCOPE itself includes the topology and the per-command seed — collisions
/// can only occur after 4096 same-scope generations between bucket changes.
///
/// Replaces the previous <c>InMemorySequenceResolver</c>. Per
/// deterministic-id.guard.md G16, sequence persistence MUST flow through
/// <see cref="ISequenceStore"/> rather than the event store.
/// </summary>
public sealed class PersistedSequenceResolver : ISequenceResolver
{
    private const int MaxSequence = 0x1000; // exclusive — X3 width

    private readonly ISequenceStore _store;

    public PersistedSequenceResolver(ISequenceStore store)
    {
        _store = store;
    }

    public async Task<int> NextAsync(string scope)
    {
        var raw = await _store.NextAsync(scope);
        return (int)(raw % MaxSequence);
    }
}
