using Microsoft.Extensions.Logging;
using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.GuardExecution.Engine;

public sealed class GuardExecutionEngine : IGuardExecutionEngine
{
    private readonly GuardRegistry _registry;
    private readonly GuardExecutor _executor;
    private readonly ILogger<GuardExecutionEngine> _logger;
    private readonly IClock _clock;

    public GuardExecutionEngine(
        GuardRegistry registry,
        GuardExecutor executor,
        ILogger<GuardExecutionEngine> logger,
        IClock clock)
    {
        _registry = registry;
        _executor = executor;
        _logger = logger;
        _clock = clock;
    }

    public IReadOnlyList<string> RegisteredGuards => _registry.RegisteredNames;

    public async Task<GuardExecutionReport> ExecuteAllAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        var guards = _registry.ResolveAll();
        _logger.LogInformation("Executing {Count} guards in {Mode} mode", guards.Count, context.Mode);

        var results = await _executor.ExecuteAllAsync(guards, context, cancellationToken);
        var status = DetermineStatus(results);

        var report = new GuardExecutionReport
        {
            GuardsExecuted = results.Count,
            Results = results,
            Status = status,
            Timestamp = _clock.UtcNowOffset
        };

        _logger.LogInformation("Guard execution complete: {Status} ({Executed} guards, {Violations} violations)",
            report.Status, report.GuardsExecuted, report.AllViolations.Count);

        return report;
    }

    public async Task<GuardExecutionReport> ExecutePhaseAsync(GuardContext context, GuardPhase phase, CancellationToken cancellationToken = default)
    {
        var guards = _registry.ResolveByPhase(phase);
        _logger.LogInformation("Executing {Count} {Phase} guards in {Mode} mode", guards.Count, phase, context.Mode);

        var results = await _executor.ExecuteAllAsync(guards, context, cancellationToken);
        var status = DetermineStatus(results);

        var report = new GuardExecutionReport
        {
            GuardsExecuted = results.Count,
            Results = results,
            Status = status,
            Timestamp = _clock.UtcNowOffset
        };

        _logger.LogInformation("Guard {Phase} phase complete: {Status} ({Executed} guards, {Violations} violations)",
            phase, report.Status, report.GuardsExecuted, report.AllViolations.Count);

        return report;
    }

    public async Task<GuardResult> ExecuteGuardAsync(string guardName, GuardContext context, CancellationToken cancellationToken = default)
    {
        var guard = _registry.Resolve(guardName)
            ?? throw new InvalidOperationException($"Guard '{guardName}' is not registered.");

        return await _executor.ExecuteAsync(guard, context, cancellationToken);
    }

    private static GuardExecutionStatus DetermineStatus(IReadOnlyList<GuardResult> results)
    {
        if (results.Any(r => r.Violations.Any(v => v.Severity is GuardSeverity.S0 or GuardSeverity.S1)))
            return GuardExecutionStatus.Fail;

        if (results.Any(r => r.Violations.Any(v => v.Severity == GuardSeverity.S2)))
            return GuardExecutionStatus.Warn;

        return GuardExecutionStatus.Pass;
    }
}
