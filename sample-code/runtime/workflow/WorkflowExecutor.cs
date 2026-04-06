using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Middleware;
using Whycespace.Runtime.Determinism;
using Whycespace.Runtime.Engine;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Observability;
using Whycespace.Runtime.Reliability;
using Whycespace.Runtime.Routing;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Runtime.Workflow;

public sealed class WorkflowExecutor
{
    private readonly EngineInvoker _engineInvoker;
    private readonly WorkflowStateManager _stateManager;
    private readonly IEventPublisher _eventPublisher;
    private readonly RetryPolicyEngine _retryPolicy;
    private readonly TimeoutManager _timeoutManager;
    private readonly DeadLetterQueue _deadLetterQueue;
    private readonly DomainRouteResolver _routeResolver;
    private readonly IIdGenerator _idGenerator;

    public WorkflowExecutor(
        EngineInvoker engineInvoker,
        WorkflowStateManager stateManager,
        IEventPublisher eventPublisher,
        RetryPolicyEngine retryPolicy,
        TimeoutManager timeoutManager,
        DeadLetterQueue deadLetterQueue,
        DomainRouteResolver routeResolver,
        IIdGenerator idGenerator)
    {
        _engineInvoker = engineInvoker;
        _stateManager = stateManager;
        _eventPublisher = eventPublisher;
        _retryPolicy = retryPolicy;
        _timeoutManager = timeoutManager;
        _deadLetterQueue = deadLetterQueue;
        _routeResolver = routeResolver;
        _idGenerator = idGenerator;
    }

    public async Task<CommandResult> ExecuteAsync(WorkflowInstance instance)
    {
        CommandResult? lastResult = null;
        var envelope = instance.CommandContext.Envelope;
        var trace = instance.CommandContext.Get<ExecutionTrace>(TracingMiddleware.ContextKeys.ExecutionTrace);

        while (instance.Status == WorkflowStatus.Running)
        {
            var stepState = instance.GetCurrentStep();
            if (stepState is null) break;

            await _stateManager.MarkStepRunningAsync(instance);

            var payload = stepState.Step.TransformedPayload
                ?? envelope.Payload;

            var engineCommandType = stepState.Step.EngineCommandType;
            var engineSpan = trace?.BeginSpan($"Engine:{engineCommandType}", TraceSpanKind.Engine);

            lastResult = await InvokeWithReliability(engineCommandType, payload, instance.CommandContext);

            engineSpan?.Complete(lastResult.Success, lastResult.ErrorMessage);

            await _stateManager.MarkStepCompletedAsync(instance, lastResult);

            var aggregateType = ExtractAggregateType(engineCommandType);
            var route = _routeResolver.Resolve(engineCommandType);
            var runtimeEvent = new RuntimeEvent
            {
                EventId = _idGenerator.DeterministicGuid($"workflow-event:{envelope.CommandId}:{engineCommandType}:{instance.CurrentStepIndex}"),
                AggregateId = Guid.Empty,
                AggregateType = aggregateType,
                EventType = lastResult.Success
                    ? $"{engineCommandType}.Completed"
                    : $"{engineCommandType}.Faulted",
                CorrelationId = envelope.CorrelationId,
                CommandId = envelope.CommandId,
                ExecutionId = instance.CommandContext.ExecutionId,
                Payload = lastResult.Data,
                Cluster = route?.Cluster,
                SubCluster = route?.SubCluster,
                App = route?.App,
                Context = route?.Context
            };

            var eventSpan = trace?.BeginSpan($"Event:{runtimeEvent.EventType}", TraceSpanKind.Event);

            await _eventPublisher.PublishAsync(runtimeEvent, instance.CommandContext.CancellationToken);

            eventSpan?.Complete(true);

            if (!lastResult.Success)
            {
                instance.Fault(lastResult.ErrorMessage ?? "Step execution failed.");
                AttachExecutionHash(instance, lastResult);
                return lastResult;
            }

            instance.AdvanceStep();
        }

        if (lastResult is not null)
            AttachExecutionHash(instance, lastResult);

        return lastResult ?? CommandResult.Fail(
            envelope.CommandId,
            "Workflow completed with no steps executed.",
            "NO_STEPS_EXECUTED",
            instance.CommandContext.Clock.UtcNowOffset);
    }

    private static void AttachExecutionHash(WorkflowInstance instance, CommandResult result)
    {
        var hash = ExecutionHash.FromWorkflow(instance, result);
        instance.CommandContext.Set(ExecutionHashKey, hash);
    }

    public const string ExecutionHashKey = "Determinism.ExecutionHash";

    /// <summary>
    /// Resolves aggregate type from engine command type via domain route.
    /// Falls back to dropping the last segment for unregistered command types.
    /// </summary>
    private string ExtractAggregateType(string engineCommandType)
    {
        return _routeResolver.ResolveAggregateType(engineCommandType);
    }

    private async Task<CommandResult> InvokeWithReliability(
        string engineCommandType,
        object payload,
        CommandContext context)
    {
        var retryResult = await _retryPolicy.ExecuteAsync(
            engineCommandType,
            async () =>
            {
                var timeoutResult = await _timeoutManager.ExecuteAsync(
                    engineCommandType,
                    async ct =>
                    {
                        var linkedContext = new CommandContext
                        {
                            Envelope = context.Envelope,
                            ExecutionId = context.ExecutionId,
                            Clock = context.Clock,
                            CancellationToken = ct
                        };

                        foreach (var kvp in context.Properties)
                            linkedContext.Properties[kvp.Key] = kvp.Value;

                        return await _engineInvoker.InvokeAsync(engineCommandType, payload, linkedContext);
                    },
                    context.CancellationToken);

                if (timeoutResult.TimedOut)
                {
                    throw new TimeoutException(timeoutResult.ErrorMessage);
                }

                return timeoutResult.Value!;
            },
            context.CancellationToken);

        if (retryResult.Success)
        {
            return retryResult.Value!;
        }

        _deadLetterQueue.Enqueue(
            context.Envelope,
            retryResult.FinalException?.Message ?? "Retries exhausted.",
            retryResult.FinalException,
            retryResult.Attempts);

        return CommandResult.Fail(
            context.Envelope.CommandId,
            $"Engine '{engineCommandType}' failed after {retryResult.TotalAttempts} attempt(s). Sent to dead-letter queue.",
            "RETRIES_EXHAUSTED",
            context.Clock.UtcNowOffset);
    }
}
