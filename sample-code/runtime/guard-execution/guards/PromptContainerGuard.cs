using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Guards;

/// <summary>
/// Enforces prompt container isolation:
/// - Prompt containers must not import engine or runtime namespaces
/// - Prompt outputs must be validated before engine consumption
/// </summary>
public sealed class PromptContainerGuard : IGuard
{
    public string Name => "PromptContainerGuard";
    public GuardCategory Category => GuardCategory.PromptContainer;
    public GuardPhase Phase => GuardPhase.PostPolicy;

    private static readonly string[] ForbiddenInPromptContainers =
    [
        "using Whycespace.Runtime",
        "using Whycespace.Engines",
        "using Whycespace.Infrastructure"
    ];

    public Task<GuardResult> EvaluateAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        if (context.Mode != GuardExecutionMode.Ci)
            return Task.FromResult(GuardResult.Pass(Name));

        var violations = new List<GuardViolation>();
        var promptFiles = context.SourceFiles
            .Where(f => f.Replace('\\', '/').Contains("prompt", StringComparison.OrdinalIgnoreCase)
                        && f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));

        foreach (var file in promptFiles)
        {
            var content = File.ReadAllText(file);

            foreach (var forbidden in ForbiddenInPromptContainers)
            {
                if (content.Contains(forbidden, StringComparison.Ordinal))
                {
                    violations.Add(new GuardViolation
                    {
                        Rule = "PROMPT_CONTAINER.FORBIDDEN_IMPORT",
                        Severity = GuardSeverity.S1,
                        File = file,
                        Description = $"Prompt container imports forbidden namespace: '{forbidden}'",
                        Expected = "Prompt containers must be isolated from runtime and engine layers",
                        Actual = $"Found '{forbidden}'",
                        Remediation = "Remove the import. Prompt containers communicate via contracts only."
                    });
                }
            }
        }

        return Task.FromResult(violations.Count == 0
            ? GuardResult.Pass(Name)
            : GuardResult.Fail(Name, violations));
    }
}
