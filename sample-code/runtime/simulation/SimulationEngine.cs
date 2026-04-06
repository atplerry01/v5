using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Middleware;
using Whycespace.Runtime.Dispatcher;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Simulation;

public sealed class SimulationEngine
{
    private readonly IMiddlewareInspector _middlewareInspector;
    private readonly WorkflowResolver _workflowResolver;
    private readonly IIdGenerator _idGenerator;

    public SimulationEngine(IMiddlewareInspector middlewareInspector, WorkflowResolver workflowResolver, IIdGenerator? idGenerator = null)
    {
        ArgumentNullException.ThrowIfNull(middlewareInspector);
        _middlewareInspector = middlewareInspector;
        _workflowResolver = workflowResolver;
        _idGenerator = idGenerator ?? DefaultGuidGenerator.Instance;
    }

    public async Task<SimulationExecutionResult> SimulateAsync(
        CommandEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var simContext = new SimulationContext { Envelope = envelope };

        simContext.Log($"Simulation started for command {envelope.CommandId} [{envelope.CommandType}]");

        try
        {
            // Phase 1: Simulate middleware pipeline (metadata only, no execution)
            simContext.Log("Phase 1: Middleware pipeline");
            SimulateMiddleware(simContext);

            // Phase 2: Simulate workflow resolution
            simContext.Log("Phase 2: Workflow resolution");
            var context = new CommandContext
            {
                Envelope = envelope,
                ExecutionId = CommandContext.GenerateExecutionId(envelope),
                Clock = SystemClock.Instance,
                CancellationToken = cancellationToken
            };

            var steps = await _workflowResolver.ResolveAsync(context);
            simContext.Log($"Resolved {steps.Length} workflow step(s)");

            foreach (var step in steps)
            {
                var span = new TraceSpan
                {
                    SpanId = _idGenerator.DeterministicGuid($"sim:span:{envelope.CommandId}:{step.EngineCommandType}"),
                    Name = $"sim:engine:{step.EngineCommandType}",
                    Kind = TraceSpanKind.Engine
                };

                simContext.CaptureSpan(span);
                simContext.Log($"  Step: {step.EngineCommandType} (engine invocation simulated, no mutation)");

                // Capture projected event
                var lastDot = step.EngineCommandType.LastIndexOf('.');
                var simAggregateType = lastDot > 0 ? step.EngineCommandType[..lastDot] : step.EngineCommandType;
                var projectedEvent = new RuntimeEvent
                {
                    EventId = _idGenerator.DeterministicGuid($"sim:event:{envelope.CommandId}:{step.EngineCommandType}"),
                AggregateId = Guid.Empty,
                    AggregateType = simAggregateType,
                    EventType = $"sim.{step.EngineCommandType}.executed",
                    CorrelationId = envelope.CorrelationId,
                    CommandId = envelope.CommandId,
                    Payload = step.TransformedPayload ?? envelope.Payload
                };

                simContext.CaptureEvent(projectedEvent);
                span.Complete(success: true);
            }

            simContext.Log("Simulation completed successfully");
            simContext.Complete();

            return BuildResult(simContext, success: true,
                projectedResult: CommandResult.Ok(envelope.CommandId));
        }
        catch (Exception ex)
        {
            simContext.Log($"Simulation faulted: {ex.Message}");
            simContext.Complete();

            return BuildResult(simContext, success: false, errorMessage: ex.Message);
        }
    }

    private void SimulateMiddleware(SimulationContext simContext)
    {
        var metadata = _middlewareInspector.GetMiddlewareMetadata();

        foreach (var entry in metadata)
        {
            var span = new TraceSpan
            {
                SpanId = _idGenerator.DeterministicGuid($"sim:middleware-span:{simContext.Envelope.CommandId}:{entry.Name}"),
                Name = $"sim:middleware:{entry.Name}",
                Kind = TraceSpanKind.Middleware
            };

            simContext.CaptureSpan(span);
            simContext.Log($"  Middleware: {entry.Name} ({entry.TypeName})");
            span.Complete(success: true);
        }
    }

    private static SimulationExecutionResult BuildResult(
        SimulationContext simContext,
        bool success,
        string? errorMessage = null,
        CommandResult? projectedResult = null)
    {
        return new SimulationExecutionResult
        {
            SimulationId = simContext.SimulationId,
            CommandId = simContext.Envelope.CommandId,
            Success = success,
            ErrorMessage = errorMessage,
            CapturedEvents = simContext.CapturedEvents.AsReadOnly(),
            CapturedSpans = simContext.CapturedSpans.AsReadOnly(),
            ExecutionLog = simContext.ExecutionLog.AsReadOnly(),
            Elapsed = (simContext.CompletedAt ?? simContext.StartedAt) - simContext.StartedAt,
            ProjectedResult = projectedResult
        };
    }
}
