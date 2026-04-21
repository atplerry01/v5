using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Stream;

namespace Whycespace.Projections.Content.Streaming.StreamCore.Stream;

/// <summary>
/// Thin adapter exposing StreamReadModel lookup for cross-BC invariant
/// evaluation (e.g. BroadcastStreamBindingPolicy). Engine handlers depend
/// on <see cref="IStreamStatusLookup"/>, not this concrete adapter.
/// </summary>
public sealed class StreamStatusLookup : IStreamStatusLookup
{
    private readonly PostgresProjectionStore<StreamReadModel> _store;

    public StreamStatusLookup(PostgresProjectionStore<StreamReadModel> store) => _store = store;

    public async Task<StreamStatusSnapshot> GetAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        var state = await _store.LoadAsync(streamId, cancellationToken);
        return state is null
            ? StreamStatusSnapshot.Missing()
            : new StreamStatusSnapshot(true, state.Status);
    }
}
