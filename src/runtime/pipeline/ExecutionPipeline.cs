using Whycespace.Runtime.ControlPlane;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Middleware;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Pipeline;

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

    public async Task<CommandResult> ExecuteAsync(object command, CommandContext context, CancellationToken cancellationToken = default)
    {
        // Step 1: Open execution scope — validate pre-conditions
        if (command is null)
            return CommandResult.ValidationFailure(
                "Execution pipeline: command is null.", ValidationFailureCategory.InputSchema);
        if (context.CorrelationId == Guid.Empty)
            return CommandResult.ValidationFailure(
                "Execution pipeline: CorrelationId is required.", ValidationFailureCategory.CommandPrecondition);

        // Step 2: Build and execute middleware pipeline
        // phase1.5-S5.2.3 / TC-1 (DISPATCHER-CT-CONTRACT-01): mirror
        // RuntimeControlPlane's token-aware closure shape so this
        // (currently unused) helper compiles through the new
        // IMiddleware contract.
        Func<CancellationToken, Task<CommandResult>> pipeline =
            ct => _dispatcher.DispatchAsync(command, context, ct);

        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var next = pipeline;
            pipeline = ct => middleware.ExecuteAsync(context, command, next, ct);
        }

        CommandResult result;
        try
        {
            result = await pipeline(cancellationToken);
        }
        catch (Exception ex)
        {
            // Step 6: Handle failure — structured error, no partial completion.
            // R-EXC-MAP-01: category sourced from RuntimeExceptionMapper so
            // retry logic (R2) sees a canonical classification.
            var mapped = RuntimeExceptionMapper.Map(ex);
            return CommandResult.Failure($"Execution pipeline failure: {ex.Message}", mapped.Category);
        }

        // Step 3: Collect emitted events
        if (!result.IsSuccess)
            return result;

        // Step 4: Validate invariants
        if (result.EventsRequirePersistence && result.EmittedEvents.Count == 0)
        {
            return CommandResult.Failure(
                "Execution pipeline: events required for persistence but none emitted.",
                RuntimeFailureCategory.InvalidState);
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
                // R-EXC-MAP-01: persistence-path exceptions classified via mapper
                // so DbException→PersistenceFailure/ConcurrencyConflict etc flow through.
                var mapped = RuntimeExceptionMapper.Map(ex);
                return CommandResult.Failure($"Event fabric failure: {ex.Message}", mapped.Category);
            }
        }

        return result;
    }
}
