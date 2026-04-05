using Whyce.Shared.Contracts.Events.Todo;
using Whyce.Shared.Contracts.Infrastructure.Messaging;

namespace Whyce.Projections.OperationalSystem.Sandbox.Todo;

public sealed class TodoProjectionConsumer : IEventConsumer
{
    private readonly TodoProjectionHandler _handler;

    public TodoProjectionConsumer(TodoProjectionHandler handler)
    {
        _handler = handler;
    }

    public async Task ConsumeAsync(object @event)
    {
        switch (@event)
        {
            case TodoCreatedEventSchema created:
                await _handler.HandleAsync(created);
                break;

            case TodoUpdatedEventSchema updated:
                await _handler.HandleAsync(updated);
                break;

            case TodoCompletedEventSchema completed:
                await _handler.HandleAsync(completed);
                break;
        }
    }
}
