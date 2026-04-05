namespace Whyce.Shared.Contracts.Infrastructure.Messaging;

public interface IEventConsumer
{
    Task ConsumeAsync(object @event);
}
