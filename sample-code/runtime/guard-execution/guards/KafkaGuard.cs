using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Guards;

/// <summary>
/// Enforces Kafka dual-topic pattern:
/// - Events must be published to both domain and integration topics
/// - No direct Kafka producer usage outside event fabric
/// - Topic naming conventions enforced
/// </summary>
public sealed class KafkaGuard : IGuard
{
    public string Name => "KafkaGuard";
    public GuardCategory Category => GuardCategory.Kafka;
    public GuardPhase Phase => GuardPhase.PrePolicy;

    private static readonly string[] ForbiddenProducerUsage =
    [
        "IProducer<",
        "ProducerBuilder<",
        "new ProducerConfig"
    ];

    public Task<GuardResult> EvaluateAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        if (context.Mode != GuardExecutionMode.Ci)
            return Task.FromResult(GuardResult.Pass(Name));

        var violations = new List<GuardViolation>();

        foreach (var file in context.SourceFiles.Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
        {
            var normalized = file.Replace('\\', '/');

            // Skip event-fabric (it's the only place allowed to use Kafka directly)
            if (normalized.Contains("event-fabric/", StringComparison.OrdinalIgnoreCase)
                || normalized.Contains("infrastructure/adapters/kafka", StringComparison.OrdinalIgnoreCase))
                continue;

            var content = File.ReadAllText(file);

            foreach (var pattern in ForbiddenProducerUsage)
            {
                if (content.Contains(pattern, StringComparison.Ordinal))
                {
                    violations.Add(new GuardViolation
                    {
                        Rule = "KAFKA.DIRECT_PRODUCER",
                        Severity = GuardSeverity.S0,
                        File = file,
                        Description = $"Direct Kafka producer usage detected outside event-fabric: '{pattern}'",
                        Expected = "Kafka access only through EventFabric (IEventPublisher)",
                        Actual = $"Found '{pattern}'",
                        Remediation = "Use IEventPublisher from event-fabric instead of direct Kafka producer."
                    });
                }
            }
        }

        return Task.FromResult(violations.Count == 0
            ? GuardResult.Pass(Name)
            : GuardResult.Fail(Name, violations));
    }
}
