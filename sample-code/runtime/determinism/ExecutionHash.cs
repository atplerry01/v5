using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Whycespace.Runtime.Command;
using Whycespace.Runtime.Workflow;

namespace Whycespace.Runtime.Determinism;

/// <summary>
/// Produces a deterministic SHA256 fingerprint of a command execution.
/// Used to verify that replayed executions produce identical results.
///
/// Hash = SHA256(ExecutionId + CommandType + Payload + StepResults + FinalOutput)
/// </summary>
public static class ExecutionHash
{
    /// <summary>
    /// Computes a hash from the command input alone (pre-execution).
    /// </summary>
    public static string FromInput(CommandEnvelope envelope)
    {
        var sb = new StringBuilder();
        sb.Append("input:");
        sb.Append(envelope.CommandId);
        sb.Append(':');
        sb.Append(envelope.CommandType);
        sb.Append(':');
        sb.Append(envelope.CorrelationId);
        sb.Append(':');
        sb.Append(envelope.Timestamp.ToString("O"));
        sb.Append(':');
        sb.Append(SerializePayload(envelope.Payload));

        return ComputeHash(sb.ToString());
    }

    /// <summary>
    /// Computes a hash from the full execution (input + steps + output).
    /// Two executions of the same command should produce the same hash
    /// if the system is deterministic.
    /// </summary>
    public static string FromExecution(
        string executionId,
        CommandEnvelope envelope,
        IReadOnlyList<StepState> steps,
        CommandResult result)
    {
        var sb = new StringBuilder();

        // Input
        sb.Append("exec:");
        sb.Append(executionId);
        sb.Append(':');
        sb.Append(envelope.CommandType);
        sb.Append(':');
        sb.Append(SerializePayload(envelope.Payload));

        // Steps
        sb.Append(":steps[");
        for (var i = 0; i < steps.Count; i++)
        {
            if (i > 0) sb.Append(',');
            var step = steps[i];
            sb.Append(step.Step.EngineCommandType);
            sb.Append('=');
            sb.Append(step.Status);
            if (step.Result?.Data is not null)
            {
                sb.Append(':');
                sb.Append(SerializePayload(step.Result.Data));
            }
        }
        sb.Append(']');

        // Output
        sb.Append(":result:");
        sb.Append(result.Success);
        if (result.Data is not null)
        {
            sb.Append(':');
            sb.Append(SerializePayload(result.Data));
        }
        if (result.ErrorCode is not null)
        {
            sb.Append(':');
            sb.Append(result.ErrorCode);
        }

        return ComputeHash(sb.ToString());
    }

    /// <summary>
    /// Computes a hash from a completed workflow instance and its result.
    /// </summary>
    public static string FromWorkflow(WorkflowInstance instance, CommandResult result)
    {
        return FromExecution(
            instance.CommandContext.ExecutionId,
            instance.CommandContext.Envelope,
            instance.Steps,
            result);
    }

    private static string ComputeHash(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash);
    }

    private static string SerializePayload(object? payload)
    {
        if (payload is null) return "null";

        try
        {
            return JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch
        {
            return payload.GetType().FullName ?? payload.ToString() ?? "unknown";
        }
    }
}
