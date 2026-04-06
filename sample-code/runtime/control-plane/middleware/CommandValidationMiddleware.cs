using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.ControlPlane.Middleware;

/// <summary>
/// Domain-aware command validation middleware.
/// Validates command-specific preconditions BEFORE dispatch to engines.
/// Uses pluggable ICommandValidator implementations registered per command type.
/// </summary>
public sealed class CommandValidationMiddleware : IMiddleware
{
    private readonly IReadOnlyDictionary<string, ICommandValidator> _validators;

    public CommandValidationMiddleware(IEnumerable<ICommandValidator> validators)
    {
        ArgumentNullException.ThrowIfNull(validators);
        _validators = validators.ToDictionary(v => v.CommandType, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        var commandType = context.Envelope.CommandType;

        if (_validators.TryGetValue(commandType, out var validator))
        {
            var validation = await validator.ValidateAsync(context, context.CancellationToken);

            if (!validation.IsValid)
            {
                return CommandResult.Fail(
                    context.Envelope.CommandId,
                    validation.ErrorMessage ?? "Command validation failed.",
                    "COMMAND_VALIDATION_FAILED",
                    context.Clock.UtcNowOffset);
            }
        }

        return await next(context);
    }
}

/// <summary>
/// Contract for domain-aware command validators.
/// Each validator targets a specific command type and validates
/// preconditions using shared contracts (no direct domain/infra access).
/// </summary>
public interface ICommandValidator
{
    string CommandType { get; }
    Task<CommandValidationResult> ValidateAsync(CommandContext context, CancellationToken cancellationToken = default);
}

public sealed record CommandValidationResult
{
    public required bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }

    public static CommandValidationResult Valid() => new() { IsValid = true };

    public static CommandValidationResult Invalid(string error, string? code = null) => new()
    {
        IsValid = false,
        ErrorMessage = error,
        ErrorCode = code
    };
}
