using Whycespace.Runtime.EventFabric;

namespace Whycespace.Runtime.Persistence;

public interface IEventStore
{
    Task AppendAsync(string streamId, RuntimeEvent @event, CancellationToken cancellationToken = default);
    Task AppendAsync(string streamId, IEnumerable<RuntimeEvent> events, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RuntimeEvent>> ReadStreamAsync(string streamId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RuntimeEvent>> ReadStreamAsync(string streamId, long fromVersion, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RuntimeEvent>> ReadAllAsync(DateTimeOffset? after = null, CancellationToken cancellationToken = default);
    Task<long> GetStreamVersionAsync(string streamId, CancellationToken cancellationToken = default);
}
