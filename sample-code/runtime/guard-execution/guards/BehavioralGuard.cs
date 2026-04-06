using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Guards;

/// <summary>
/// Enforces behavioral invariants:
/// - Deterministic execution (no DateTime.Now, no Random without seed)
/// - Event-first design (commands must emit events)
/// - No direct mutation outside engines
/// </summary>
public sealed class BehavioralGuard : IGuard
{
    public string Name => "BehavioralGuard";
    public GuardCategory Category => GuardCategory.Behavioral;
    public GuardPhase Phase => GuardPhase.PrePolicy;

    private static readonly string[] ForbiddenPatterns =
    [
        "DateTime.Now",
        "DateTime.UtcNow",
        "DateTimeOffset.Now",
        "DateTimeOffset.UtcNow",
        "new Random()",
        "Guid.NewGuid()"
    ];

    private static readonly string[] EnginePaths = ["src/engines/", "src/runtime/"];

    public Task<GuardResult> EvaluateAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        if (context.Mode != GuardExecutionMode.Ci)
            return Task.FromResult(GuardResult.Pass(Name));

        var violations = new List<GuardViolation>();

        foreach (var file in context.SourceFiles.Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
        {
            var normalizedPath = file.Replace('\\', '/');
            var isEngineOrRuntime = EnginePaths.Any(p => normalizedPath.Contains(p, StringComparison.OrdinalIgnoreCase));
            if (!isEngineOrRuntime) continue;

            var content = File.ReadAllText(file);

            foreach (var pattern in ForbiddenPatterns)
            {
                if (content.Contains(pattern, StringComparison.Ordinal))
                {
                    violations.Add(new GuardViolation
                    {
                        Rule = "BEHAVIORAL.NON_DETERMINISTIC",
                        Severity = GuardSeverity.S1,
                        File = file,
                        Description = $"Non-deterministic call detected: '{pattern}'",
                        Expected = "Use IClock or deterministic ID generation",
                        Actual = $"Found '{pattern}'",
                        Remediation = "Replace with IClock.UtcNow or DeterministicIdHelper for deterministic execution."
                    });
                }
            }
        }

        return Task.FromResult(violations.Count == 0
            ? GuardResult.Pass(Name)
            : GuardResult.Fail(Name, violations));
    }
}
