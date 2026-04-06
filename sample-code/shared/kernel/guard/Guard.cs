using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Whycespace.Shared.Kernel.Guard;

public static class Guard
{
    public static T AgainstNull<T>(
        [NotNull] T? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }

    public static string AgainstEmpty(
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value;
    }

    public static T AgainstInvalid<T>(
        T value,
        Func<T, bool> predicate,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (!predicate(value))
            throw new ArgumentException(
                message ?? $"Value '{paramName}' does not satisfy the required condition.",
                paramName);
        return value;
    }

    public static Guid AgainstDefault(
        Guid value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value == Guid.Empty)
            throw new ArgumentException(
                $"'{paramName}' must not be an empty GUID.",
                paramName);
        return value;
    }

    public static decimal AgainstNegativeOrZero(
        decimal value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                $"'{paramName}' must be greater than zero.");
        return value;
    }

    public static decimal AgainstNegative(
        decimal value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                $"'{paramName}' must not be negative.");
        return value;
    }

    public static DateTimeOffset AgainstPast(
        DateTimeOffset value,
        DateTimeOffset now,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value <= now)
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                $"'{paramName}' must be in the future.");
        return value;
    }

    public static void AgainstTerminalState<TStatus>(
        TStatus status,
        Func<TStatus, bool> isTerminal,
        string aggregateType,
        string action,
        Guid? aggregateId = null)
    {
        if (isTerminal(status))
            throw new ArgumentException(
                $"[{aggregateType}] Cannot '{action}' — aggregate is in terminal state '{status}'.");
    }
}
