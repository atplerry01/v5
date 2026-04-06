using System.Text.Json;
using Whycespace.Runtime.Command;
using Whycespace.Shared.Contracts.Workflow;

namespace Whycespace.Runtime.Workflow;

/// <summary>
/// Resolves IWorkflowContext from CommandContext.
/// Extracts workflow metadata from command payload and headers.
/// Returns null for non-workflow commands.
/// </summary>
public sealed class WorkflowContextResolver
{
    public IWorkflowContext? Resolve(CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var commandType = context.Envelope.CommandType;

        // Extract workflow metadata from headers or context properties
        var headers = context.Envelope.Metadata.Headers;
        var workflowId = GetFromHeadersOrProperties(context, headers, "X-Workflow-Id", "Workflow.WorkflowId");
        var stepId = GetFromHeadersOrProperties(context, headers, "X-Step-Id", "Workflow.StepId");

        // If no explicit workflow context, check if it's a workflow-type command
        if (workflowId is null && !IsWorkflowCommand(commandType))
            return null;

        // Extract from payload if not in headers
        var payload = context.Envelope.Payload;
        var json = payload is JsonElement je ? je : ParsePayload(payload);

        workflowId ??= GetStringFromJson(json, "WorkflowId") ?? context.ExecutionId;
        stepId ??= GetStringFromJson(json, "StepId") ?? commandType;

        var workflowType = ResolveWorkflowType(commandType);
        var stepType = ResolveStepType(commandType);
        var state = GetFromHeadersOrProperties(context, headers, "X-Workflow-State", "Workflow.State") ?? "executing";
        var transition = GetFromHeadersOrProperties(context, headers, "X-Workflow-Transition", "Workflow.Transition")
            ?? ResolveTransition(commandType);

        return new ResolvedWorkflowContext
        {
            WorkflowId = workflowId,
            WorkflowType = workflowType,
            StepId = stepId,
            StepType = stepType,
            State = state,
            Transition = transition
        };
    }

    private static bool IsWorkflowCommand(string commandType)
    {
        return commandType.StartsWith("workflow.", StringComparison.OrdinalIgnoreCase)
            || commandType.Contains(".step.", StringComparison.OrdinalIgnoreCase)
            || commandType.Contains(".orchestrate", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveWorkflowType(string commandType)
    {
        var parts = commandType.Split('.');
        return parts.Length >= 2 ? $"{parts[0]}.{parts[1]}" : commandType;
    }

    private static string ResolveStepType(string commandType)
    {
        var parts = commandType.Split('.');
        return parts.Length >= 3 ? parts[^1] : "execute";
    }

    private static string ResolveTransition(string commandType)
    {
        var parts = commandType.Split('.');
        var operation = parts.Length >= 3 ? parts[^1].ToLowerInvariant() : "execute";
        return operation switch
        {
            "start" or "begin" or "create" => "start",
            "complete" or "finish" or "done" => "complete",
            "fail" or "error" or "fault" => "fault",
            "cancel" or "abort" => "cancel",
            "retry" or "resume" => "retry",
            _ => "execute"
        };
    }

    private static string? GetFromHeadersOrProperties(
        CommandContext context,
        IReadOnlyDictionary<string, string> headers,
        string headerKey,
        string propertyKey)
    {
        if (headers.TryGetValue(headerKey, out var headerVal) && !string.IsNullOrEmpty(headerVal))
            return headerVal;
        return context.Get<string>(propertyKey);
    }

    private static JsonElement? ParsePayload(object? payload)
    {
        if (payload is null) return null;
        var s = JsonSerializer.Serialize(payload);
        return JsonDocument.Parse(s).RootElement;
    }

    private static string? GetStringFromJson(JsonElement? json, string prop)
    {
        if (json is null) return null;
        return json.Value.TryGetProperty(prop, out var v) ? v.GetString() : null;
    }
}

public sealed record ResolvedWorkflowContext : IWorkflowContext
{
    public required string WorkflowId { get; init; }
    public required string WorkflowType { get; init; }
    public required string StepId { get; init; }
    public required string StepType { get; init; }
    public required string State { get; init; }
    public required string Transition { get; init; }
}
