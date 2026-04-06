using Microsoft.Extensions.Logging;
using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.GuardExecution.Runtime;

/// <summary>
/// Runtime executor: runs a subset of critical guards for runtime enforcement.
/// Used by GuardMiddleware to evaluate only runtime-applicable guards.
/// </summary>
public sealed class GuardRuntimeExecutor
{
    private readonly GuardRegistry _registry;
    private readonly GuardExecutor _executor;
    private readonly ILogger<GuardRuntimeExecutor> _logger;
    private readonly IClock _clock;

    private static readonly GuardCategory[] RuntimeCategories =
    [
        GuardCategory.Runtime,
        GuardCategory.Policy,
        GuardCategory.PolicyBinding,
        GuardCategory.Behavioral
    ];

    public GuardRuntimeExecutor(
        GuardRegistry registry,
        GuardExecutor executor,
        ILogger<GuardRuntimeExecutor> logger,
        IClock clock)
    {
        _registry = registry;
        _executor = executor;
        _logger = logger;
        _clock = clock;
    }

    public async Task<GuardExecutionReport> ExecuteRuntimeGuardsAsync(
        GuardContext context,
        CancellationToken cancellationToken = default)
    {
        var guards = RuntimeCategories
            .SelectMany(c => _registry.ResolveByCategory(c))
            .ToList();

        _logger.LogDebug("Executing {Count} runtime guards", guards.Count);

        var results = await _executor.ExecuteAllAsync(guards, context, cancellationToken);

        var status = results.Any(r => r.HasBlockingViolations)
            ? GuardExecutionStatus.Fail
            : results.Any(r => r.Violations.Any(v => v.Severity == GuardSeverity.S2))
                ? GuardExecutionStatus.Warn
                : GuardExecutionStatus.Pass;

        return new GuardExecutionReport
        {
            GuardsExecuted = results.Count,
            Results = results,
            Status = status,
            Timestamp = _clock.UtcNowOffset
        };
    }
}
