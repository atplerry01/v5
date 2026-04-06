using Microsoft.Extensions.Logging;
using Whycespace.Runtime.GuardExecution.Contracts;

namespace Whycespace.Runtime.GuardExecution.Engine;

public sealed class GuardExecutor
{
    private readonly ILogger<GuardExecutor> _logger;

    public GuardExecutor(ILogger<GuardExecutor> logger)
    {
        _logger = logger;
    }

    public async Task<GuardResult> ExecuteAsync(IGuard guard, GuardContext context, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing guard: {GuardName} [{Category}]", guard.Name, guard.Category);
            var result = await guard.EvaluateAsync(context, cancellationToken);
            _logger.LogDebug("Guard {GuardName}: {Status} ({ViolationCount} violations)",
                guard.Name, result.Passed ? "PASS" : "FAIL", result.Violations.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Guard {GuardName} threw an exception during evaluation", guard.Name);
            return GuardResult.Fail(guard.Name, [
                new GuardViolation
                {
                    Rule = "GUARD.EXECUTION_ERROR",
                    Severity = GuardSeverity.S0,
                    File = "N/A",
                    Description = $"Guard execution failed: {ex.Message}",
                    Remediation = "Fix the guard implementation or the condition that caused the error."
                }
            ]);
        }
    }

    public async Task<IReadOnlyList<GuardResult>> ExecuteAllAsync(
        IReadOnlyList<IGuard> guards,
        GuardContext context,
        CancellationToken cancellationToken)
    {
        var results = new List<GuardResult>(guards.Count);

        foreach (var guard in guards)
        {
            var result = await ExecuteAsync(guard, context, cancellationToken);
            results.Add(result);

            // S0 = fail immediately, stop further execution
            if (result.Violations.Any(v => v.Severity == GuardSeverity.S0))
            {
                _logger.LogWarning("S0 violation detected in {GuardName}. Halting guard execution.", guard.Name);
                break;
            }
        }

        return results;
    }
}
