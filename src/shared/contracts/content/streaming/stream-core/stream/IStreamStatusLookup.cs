namespace Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Stream;

/// <summary>
/// Cross-BC state lookup for the Stream projection. Used by engines that
/// must evaluate cross-system invariants involving Stream state (e.g.
/// BroadcastStreamBindingPolicy) without taking a direct infrastructure
/// dependency.
///
/// Implementation is a thin adapter over the StreamReadModel projection
/// store, registered in the composition root.
/// </summary>
public interface IStreamStatusLookup
{
    Task<StreamStatusSnapshot> GetAsync(Guid streamId, CancellationToken cancellationToken = default);
}

public readonly record struct StreamStatusSnapshot(bool Exists, string Status)
{
    public static StreamStatusSnapshot Missing() => new(false, string.Empty);
    public bool IsTerminal => Status == "Ended" || Status == "Archived";
}
