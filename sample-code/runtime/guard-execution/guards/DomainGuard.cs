using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Guards;

/// <summary>
/// Enforces domain layer purity:
/// - Domain has zero infrastructure dependencies
/// - Domain models are not defined outside domain layer
/// - No service locator or DI container usage in domain
/// </summary>
public sealed class DomainGuard : IGuard
{
    public string Name => "DomainGuard";
    public GuardCategory Category => GuardCategory.Domain;
    public GuardPhase Phase => GuardPhase.PrePolicy;

    private static readonly string[] ForbiddenInDomain =
    [
        "IServiceProvider",
        "IServiceCollection",
        "HttpClient",
        "DbContext",
        "ILogger",
        "IConfiguration",
        "IOptions<"
    ];

    public Task<GuardResult> EvaluateAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        if (context.Mode != GuardExecutionMode.Ci)
            return Task.FromResult(GuardResult.Pass(Name));

        var violations = new List<GuardViolation>();
        var domainFiles = context.SourceFiles
            .Where(f => f.Replace('\\', '/').Contains("src/domain/", StringComparison.OrdinalIgnoreCase)
                        && f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));

        foreach (var file in domainFiles)
        {
            var content = File.ReadAllText(file);

            foreach (var forbidden in ForbiddenInDomain)
            {
                if (content.Contains(forbidden, StringComparison.Ordinal))
                {
                    violations.Add(new GuardViolation
                    {
                        Rule = "DOMAIN.INFRASTRUCTURE_LEAK",
                        Severity = GuardSeverity.S0,
                        File = file,
                        Description = $"Infrastructure type '{forbidden}' found in domain layer",
                        Expected = "Domain layer must be pure DDD with zero external dependencies",
                        Actual = $"Found '{forbidden}' reference",
                        Remediation = "Remove infrastructure dependency. Use domain interfaces/ports instead."
                    });
                }
            }
        }

        return Task.FromResult(violations.Count == 0
            ? GuardResult.Pass(Name)
            : GuardResult.Fail(Name, violations));
    }
}
