using Microsoft.Extensions.Logging;
using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Ci;

/// <summary>
/// CI entry point: loads all guards, executes in deterministic order,
/// aggregates results, and returns exit code.
/// </summary>
public sealed class GuardCiRunner
{
    private readonly IGuardExecutionEngine _engine;
    private readonly GuardReportFormatter _formatter;
    private readonly ILogger<GuardCiRunner> _logger;

    public GuardCiRunner(
        IGuardExecutionEngine engine,
        GuardReportFormatter formatter,
        ILogger<GuardCiRunner> logger)
    {
        _engine = engine;
        _formatter = formatter;
        _logger = logger;
    }

    /// <summary>
    /// Runs all guards against the codebase. Returns exit code (0=pass, 1=fail, 2=warn).
    /// </summary>
    public async Task<int> RunAsync(string solutionRoot, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Guard CI Runner starting — scanning {Root}", solutionRoot);

        var sourceFiles = DiscoverSourceFiles(solutionRoot);
        _logger.LogInformation("Discovered {Count} source files", sourceFiles.Count);

        var context = GuardContext.ForCi(solutionRoot, sourceFiles);
        var report = await _engine.ExecuteAllAsync(context, cancellationToken);

        var formattedReport = _formatter.Format(report);
        _logger.LogInformation("{Report}", formattedReport);

        return report.Status switch
        {
            GuardExecutionStatus.Fail => 1,
            GuardExecutionStatus.Warn => 2,
            GuardExecutionStatus.Pass => 0,
            _ => 1
        };
    }

    private static IReadOnlyList<string> DiscoverSourceFiles(string root)
    {
        if (!Directory.Exists(root))
            return [];

        return Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                        && !f.Contains("obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                        && !f.Contains(".git" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();
    }
}
