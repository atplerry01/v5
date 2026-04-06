using Whycespace.Shared.Contracts.Systems;

namespace Whycespace.Systems.Downstream;

/// <summary>
/// Resolves the correct downstream process handler for a given command type.
/// Registered handlers are matched by command prefix.
/// </summary>
public sealed class ProcessHandlerRegistry : IProcessHandlerRegistry
{
    private readonly IReadOnlyList<IProcessHandler> _handlers;

    public ProcessHandlerRegistry(IEnumerable<IProcessHandler> handlers)
    {
        _handlers = handlers.ToList().AsReadOnly();
    }

    public IProcessHandler Resolve(string commandType)
    {
        var handler = _handlers.FirstOrDefault(h => h.CanHandle(commandType));

        if (handler is null)
            throw new InvalidOperationException(
                $"No downstream handler registered for command type: {commandType}");

        return handler;
    }
}
