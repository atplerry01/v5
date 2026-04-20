using System.Data.Common;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.ControlPlane;

/// <summary>
/// R1 §2 — runtime exception mapping discipline. Single seam that maps an
/// arbitrary <see cref="Exception"/> to a canonical
/// <see cref="RuntimeFailureCategory"/> and a safe, caller-facing error string.
///
/// The mapper never returns stack traces, inner-exception chains, or internal
/// type names — only canonical user-facing messages. Diagnostic detail belongs
/// in structured logs, not response bodies.
///
/// Used by the control plane to convert unhandled middleware exceptions into
/// <see cref="CommandResult"/> rejections with the correct category for R2
/// retry logic.
/// </summary>
public static class RuntimeExceptionMapper
{
    public readonly record struct MappedFailure(
        RuntimeFailureCategory Category,
        string SafeMessage,
        PersistenceFailureCategory? PersistenceCategory = null);

    public static MappedFailure Map(Exception ex) => ex switch
    {
        OperationCanceledException => new MappedFailure(
            RuntimeFailureCategory.Cancellation,
            "Operation was cancelled."),

        TimeoutException => new MappedFailure(
            RuntimeFailureCategory.Timeout,
            "Operation timed out."),

        // R2.A.D.3c / R-POSTGRES-POOL-BREAKER-OPEN-SEMANTICS-01: when the
        // shared "postgres-pool" (or any other) circuit breaker is Open,
        // the dependency is treated as unavailable. Callers see a safe
        // "Dependency unavailable." message; API edge handlers can also
        // consult the typed exception directly for Retry-After.
        CircuitBreakerOpenException => new MappedFailure(
            RuntimeFailureCategory.DependencyUnavailable,
            "Dependency unavailable."),

        DbException dbex => MapDbException(dbex),

        InvalidOperationException => new MappedFailure(
            RuntimeFailureCategory.InvalidState,
            "Operation is not valid in the current state."),

        UnauthorizedAccessException => new MappedFailure(
            RuntimeFailureCategory.AuthorizationDenied,
            "Authorization denied."),

        ArgumentException => new MappedFailure(
            RuntimeFailureCategory.ValidationFailed,
            "Command arguments are invalid."),

        _ => new MappedFailure(
            RuntimeFailureCategory.ExecutionFailure,
            "Execution failed.")
    };

    /// <summary>
    /// Map the exception and translate into a rejected <see cref="CommandResult"/>
    /// preserving correlation / audit data from the in-flight context.
    /// </summary>
    public static CommandResult ToCommandResult(Exception ex, Guid correlationId)
    {
        var mapped = Map(ex);
        if (mapped.PersistenceCategory is { } pcat)
        {
            return new CommandResult
            {
                IsSuccess = false,
                Error = mapped.SafeMessage,
                FailureCategory = mapped.Category,
                PersistenceCategory = pcat,
                CorrelationId = correlationId
            };
        }

        return new CommandResult
        {
            IsSuccess = false,
            Error = mapped.SafeMessage,
            FailureCategory = mapped.Category,
            CorrelationId = correlationId
        };
    }

    private static MappedFailure MapDbException(DbException ex)
    {
        var sqlState = ex.SqlState;

        return sqlState switch
        {
            // PostgreSQL SQLSTATE codes — documented in Npgsql; literal values
            // used here to avoid taking a provider dependency from the mapper.
            "23505" => new MappedFailure(
                RuntimeFailureCategory.PersistenceFailure,
                "A record with the same key already exists.",
                PersistenceFailureCategory.Duplicate),

            "23503" or "23502" or "23514" => new MappedFailure(
                RuntimeFailureCategory.PersistenceFailure,
                "Persistence integrity constraint violated.",
                PersistenceFailureCategory.IntegrityViolation),

            "40001" or "40P01" => new MappedFailure(
                RuntimeFailureCategory.ConcurrencyConflict,
                "Concurrent modification detected. Retry the operation.",
                PersistenceFailureCategory.OptimisticConflict),

            "57014" => new MappedFailure(
                RuntimeFailureCategory.Timeout,
                "Database operation timed out.",
                PersistenceFailureCategory.Timeout),

            "53100" or "53200" or "53300" or "53400" => new MappedFailure(
                RuntimeFailureCategory.ResourceExhausted,
                "Database resources exhausted.",
                PersistenceFailureCategory.Exhausted),

            "08000" or "08003" or "08006" or "08001" or "08004" => new MappedFailure(
                RuntimeFailureCategory.DependencyUnavailable,
                "Database connection failed.",
                PersistenceFailureCategory.Unavailable),

            _ => new MappedFailure(
                RuntimeFailureCategory.PersistenceFailure,
                "Persistence operation failed.",
                PersistenceFailureCategory.Unknown)
        };
    }
}
