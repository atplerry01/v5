namespace Whyce.Shared.Contracts.Infrastructure.Messaging;

public interface IOutbox
{
    Task EnqueueAsync(Guid correlationId, IReadOnlyList<object> events, string topic);
}
