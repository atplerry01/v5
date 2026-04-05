namespace Whyce.Shared.Contracts.Infrastructure.Persistence;

public interface IEventStore
{
    Task<IReadOnlyList<object>> LoadEventsAsync(Guid aggregateId);
    Task AppendEventsAsync(Guid aggregateId, IReadOnlyList<object> events, int expectedVersion);
}
