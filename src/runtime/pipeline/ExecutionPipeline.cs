using Whyce.Runtime.EventFabric;
using Whyce.Runtime.Middleware;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Pipeline;

/// <summary>
/// Execution Pipeline — orchestrates the full runtime execution flow.
///
/// Responsibilities:
/// 1. Open execution scope
/// 2. Execute middleware chain → dispatcher → engine
/// 3. Collect emitted events
/// 4. Validate invariants (events must be present for successful mutations)
/// 5. Call EventFabric (persist → chain → projection → outbox)
/// 6. Handle failure (structured error, no partial completion)
/// </summary>
public sealed class ExecutionPipeline
{
    private readonly IReadOnlyList<IMiddleware> _middlewares;
    private readonly ICommandDispatcher _dispatcher;
    private readonly IEventFabric _eventFabric;

    public ExecutionPipeline(
        IReadOnlyList<IMiddleware> middlewares,
        ICommandDispatcher dispatcher,
        IEventFabric eventFabric)
    {
        _middlewares = middlewares;
        _dispatcher = dispatcher;
        _eventFabric = eventFabric;
    }

    public async Task<CommandResult> ExecuteAsync(object command, CommandContext context)
    {
        // Step 1: Open execution scope — validate pre-conditions
        if (command is null)
            return CommandResult.Failure("Execution pipeline: command is null.");
        if (context.CorrelationId == Guid.Empty)
            return CommandResult.Failure("Execution pipeline: CorrelationId is required.");

        // Step 2: Build and execute middleware pipeline
        Func<Task<CommandResult>> pipeline = () => _dispatcher.DispatchAsync(command, context);

        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var next = pipeline;
            pipeline = () => middleware.ExecuteAsync(context, command, next);
        }

        CommandResult result;
        try
        {
            result = await pipeline();
        }
        catch (Exception ex)
        {
            // Step 6: Handle failure — structured error, no partial completion
            return CommandResult.Failure($"Execution pipeline failure: {ex.Message}");
        }

        // Step 3: Collect emitted events
        if (!result.IsSuccess)
            return result;

        // Step 4: Validate invariants
        if (result.EventsRequirePersistence && result.EmittedEvents.Count == 0)
        {
            return CommandResult.Failure(
                "Execution pipeline: events required for persistence but none emitted.");
        }

        // Step 5: Call EventFabric (single, non-bypassable)
        if (result.EventsRequirePersistence && result.EmittedEvents.Count > 0)
        {
            try
            {
                await _eventFabric.ProcessAsync(result.EmittedEvents, context);
            }
            catch (Exception ex)
            {
                return CommandResult.Failure($"Event fabric failure: {ex.Message}");
            }
        }

        return result;
    }
}
