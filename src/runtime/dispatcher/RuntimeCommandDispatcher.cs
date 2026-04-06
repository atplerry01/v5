using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Runtime.WorkflowState;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whyce.Runtime.Dispatcher;

/// <summary>
/// Routes commands to appropriate handlers: workflow orchestration (T1M) or
/// direct engine execution (T2E). The dispatcher is the terminal step in the
/// middleware pipeline — reached only after all guards and policy have passed.
/// </summary>
public sealed class RuntimeCommandDispatcher : ICommandDispatcher
{
    private readonly IEngineRegistry _engineRegistry;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventStore _eventStore;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IWorkflowRegistry _workflowRegistry;
    private readonly IWorkflowStateRepository _workflowStateRepository;
    private readonly IClock _clock;

    public RuntimeCommandDispatcher(
        IEngineRegistry engineRegistry,
        IServiceProvider serviceProvider,
        IEventStore eventStore,
        IWorkflowEngine workflowEngine,
        IWorkflowRegistry workflowRegistry,
        IWorkflowStateRepository workflowStateRepository,
        IClock clock)
    {
        _engineRegistry = engineRegistry;
        _serviceProvider = serviceProvider;
        _eventStore = eventStore;
        _workflowEngine = workflowEngine;
        _workflowRegistry = workflowRegistry;
        _workflowStateRepository = workflowStateRepository;
        _clock = clock;
    }

    public async Task<CommandResult> DispatchAsync(object command, CommandContext context)
    {
        if (command is WorkflowStartCommand workflowCommand)
        {
            return await ExecuteWorkflowAsync(workflowCommand, context);
        }

        if (command is WorkflowResumeCommand resumeCommand)
        {
            return await ResumeWorkflowAsync(resumeCommand, context);
        }

        return await ExecuteEngineAsync(command, context);
    }

    private async Task<CommandResult> ExecuteWorkflowAsync(WorkflowStartCommand command, CommandContext context)
    {
        var stepTypes = _workflowRegistry.Resolve(command.WorkflowName);
        if (stepTypes is null || stepTypes.Count == 0)
        {
            return CommandResult.Failure($"No workflow registered for '{command.WorkflowName}'.");
        }

        var definition = new WorkflowDefinition
        {
            Name = command.WorkflowName,
            Steps = stepTypes.Select((type, index) =>
            {
                var step = (IWorkflowStep)_serviceProvider.GetRequiredService(type);
                return new WorkflowStepDefinition
                {
                    StepId = ComputeStepId(command.WorkflowName, step.Name, index),
                    StepName = step.Name,
                    StepType = step.StepType,
                    StepHandlerType = type
                };
            }).ToList()
        };

        var now = _clock.UtcNow;
        var workflowIdString = command.Id.ToString();

        var stateRecord = new WorkflowStateRecord
        {
            WorkflowId = workflowIdString,
            WorkflowName = command.WorkflowName,
            CurrentStepIndex = 0,
            ExecutionHash = string.Empty,
            Status = "Running",
            SerializedState = "{}",
            CreatedAt = now,
            UpdatedAt = now
        };
        await _workflowStateRepository.SaveAsync(stateRecord);

        var observer = new WorkflowStateObserver(_workflowStateRepository, _clock);

        var executionContext = new WorkflowExecutionContext
        {
            WorkflowId = command.Id,
            CorrelationId = context.CorrelationId,
            WorkflowName = command.WorkflowName,
            Payload = command.Payload,
            IdentityId = context.IdentityId,
            PolicyDecision = context.PolicyDecisionHash,
            StepObserver = observer
        };

        var result = await _workflowEngine.ExecuteAsync(definition, executionContext);

        return result.IsSuccess
            ? CommandResult.Success(result.EmittedEvents, result.Output, eventsRequirePersistence: false)
            : CommandResult.Failure(
                $"Workflow '{command.WorkflowName}' failed at step '{result.FailedStep}': {result.Error}");
    }

    private async Task<CommandResult> ResumeWorkflowAsync(WorkflowResumeCommand command, CommandContext context)
    {
        var stateRecord = await _workflowStateRepository.GetAsync(command.WorkflowId);
        if (stateRecord is null)
        {
            return CommandResult.Failure($"No workflow state found for WorkflowId '{command.WorkflowId}'.");
        }

        if (stateRecord.Status is not "Failed" and not "Running")
        {
            return CommandResult.Failure(
                $"Workflow '{command.WorkflowId}' cannot be resumed from status '{stateRecord.Status}'.");
        }

        var stepTypes = _workflowRegistry.Resolve(stateRecord.WorkflowName);
        if (stepTypes is null || stepTypes.Count == 0)
        {
            return CommandResult.Failure($"No workflow registered for '{stateRecord.WorkflowName}'.");
        }

        var definition = new WorkflowDefinition
        {
            Name = stateRecord.WorkflowName,
            Steps = stepTypes.Select((type, index) =>
            {
                var step = (IWorkflowStep)_serviceProvider.GetRequiredService(type);
                return new WorkflowStepDefinition
                {
                    StepId = ComputeStepId(stateRecord.WorkflowName, step.Name, index),
                    StepName = step.Name,
                    StepType = step.StepType,
                    StepHandlerType = type
                };
            }).ToList()
        };

        var resumeIndex = stateRecord.Status == "Failed"
            ? stateRecord.CurrentStepIndex
            : stateRecord.CurrentStepIndex + 1;

        var restoredState = WorkflowStateSerializer.Deserialize(stateRecord.SerializedState);

        stateRecord.Status = "Running";
        stateRecord.UpdatedAt = _clock.UtcNow;
        await _workflowStateRepository.UpdateAsync(stateRecord);

        var observer = new WorkflowStateObserver(_workflowStateRepository, _clock);

        var executionContext = new WorkflowExecutionContext
        {
            WorkflowId = Guid.Parse(command.WorkflowId),
            CorrelationId = context.CorrelationId,
            WorkflowName = stateRecord.WorkflowName,
            Payload = new object(),
            IdentityId = context.IdentityId,
            PolicyDecision = context.PolicyDecisionHash,
            CurrentStepIndex = resumeIndex,
            ExecutionHash = stateRecord.ExecutionHash,
            StepObserver = observer
        };

        foreach (var kvp in restoredState)
        {
            executionContext.State[kvp.Key] = kvp.Value;
        }

        var result = await _workflowEngine.ExecuteAsync(definition, executionContext);

        return result.IsSuccess
            ? CommandResult.Success(result.EmittedEvents, result.Output, eventsRequirePersistence: false)
            : CommandResult.Failure(
                $"Workflow '{stateRecord.WorkflowName}' failed at step '{result.FailedStep}': {result.Error}");
    }

    private static string ComputeStepId(string workflowName, string stepName, int index)
    {
        var input = $"{workflowName}:{stepName}:{index}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }

    private async Task<CommandResult> ExecuteEngineAsync(object command, CommandContext context)
    {
        var engineType = _engineRegistry.ResolveEngine(command.GetType());
        if (engineType is null)
        {
            return CommandResult.Failure($"No engine registered for {command.GetType().Name}.");
        }

        var engine = (IEngine)_serviceProvider.GetRequiredService(engineType);

        var engineContext = new EngineContext(
            command,
            context.AggregateId,
            async (type, aggregateId) =>
            {
                var aggregate = (AggregateRoot)Activator.CreateInstance(type, nonPublic: true)!;
                var events = await _eventStore.LoadEventsAsync(aggregateId);
                aggregate.LoadFromHistory(events);
                return aggregate;
            });

        await engine.ExecuteAsync(engineContext);

        var emittedEvents = engineContext.EmittedEvents;
        if (emittedEvents.Count == 0)
        {
            return CommandResult.Success([]);
        }

        // Events returned to ControlPlane for persistence (Persist → Chain → Outbox)
        return CommandResult.Success(emittedEvents, eventsRequirePersistence: true);
    }
}
