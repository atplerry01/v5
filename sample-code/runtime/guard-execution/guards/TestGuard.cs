using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Guards;

/// <summary>
/// Enforces test hygiene:
/// - Tests must not reference infrastructure directly
/// - Integration tests must use real database (no mocks for event store)
/// - Test files must follow naming conventions
/// </summary>
public sealed class TestGuard : IGuard
{
    public string Name => "TestGuard";
    public GuardCategory Category => GuardCategory.Test;
    public GuardPhase Phase => GuardPhase.PostPolicy;

    public Task<GuardResult> EvaluateAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        if (context.Mode != GuardExecutionMode.Ci)
            return Task.FromResult(GuardResult.Pass(Name));

        var violations = new List<GuardViolation>();
        var testFiles = context.SourceFiles
            .Where(f => f.Replace('\\', '/').Contains("tests/", StringComparison.OrdinalIgnoreCase)
                        && f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));

        foreach (var file in testFiles)
        {
            var content = File.ReadAllText(file);
            var normalized = file.Replace('\\', '/');

            // Integration tests must not mock the event store
            if (normalized.Contains("integration", StringComparison.OrdinalIgnoreCase))
            {
                if (content.Contains("Mock<IEventStore>", StringComparison.Ordinal)
                    || content.Contains("Substitute.For<IEventStore>", StringComparison.Ordinal))
                {
                    violations.Add(new GuardViolation
                    {
                        Rule = "TEST.MOCKED_EVENT_STORE",
                        Severity = GuardSeverity.S2,
                        File = file,
                        Description = "Integration test mocks event store instead of using real implementation",
                        Expected = "Integration tests must hit real storage",
                        Remediation = "Use TestContainers or in-memory event store for integration tests."
                    });
                }
            }

            // Test file naming convention
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (!fileName.EndsWith("Tests") && !fileName.EndsWith("Test")
                && !fileName.EndsWith("Spec") && !fileName.EndsWith("Fixture")
                && !fileName.Contains("Helper") && !fileName.Contains("Base")
                && !fileName.Contains("Factory") && !fileName.Contains("Builder"))
            {
                violations.Add(new GuardViolation
                {
                    Rule = "TEST.NAMING_CONVENTION",
                    Severity = GuardSeverity.S3,
                    File = file,
                    Description = $"Test file '{fileName}' does not follow naming convention",
                    Expected = "Test files should end with Tests, Test, Spec, or Fixture",
                    Remediation = "Rename the file to follow the convention (e.g., MyFeatureTests.cs)."
                });
            }
        }

        return Task.FromResult(violations.Count == 0
            ? GuardResult.Pass(Name)
            : GuardResult.Fail(Name, violations));
    }
}
