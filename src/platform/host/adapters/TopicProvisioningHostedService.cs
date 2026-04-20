using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// R2.E.4 / R-TOPIC-PROVISIONING-HOSTED-SERVICE-01 — startup-time topic
/// alignment verifier. On <see cref="StartAsync"/>:
///
/// <list type="number">
///   <item>If <c>VerifyTopicsOnStartup == false</c>, return immediately
///         (test-friendly opt-out).</item>
///   <item>Build an <see cref="IAdminClient"/> against the configured
///         broker.</item>
///   <item>Call <see cref="KafkaTopicVerifier.FindMissingTopicsAsync"/>
///         with <see cref="KafkaCanonicalTopics.All"/>.</item>
///   <item>Log success (INFO), missing topics (WARN per topic +
///         summary), or broker failure (ERROR).</item>
///   <item>If <c>FailIfTopicsMissing == true</c> AND missing.Count &gt; 0,
///         throw <see cref="InvalidOperationException"/> so the host
///         does not reach "ready" until infrastructure is reconciled.</item>
/// </list>
///
/// <see cref="StopAsync"/> is a no-op — the service has no long-running
/// state. Running as the first <see cref="IHostedService"/> registered
/// by <c>KafkaInfrastructureModule.AddMessaging</c> means verification
/// completes (or fails) before projection / saga / integration workers
/// subscribe.
///
/// A broker-unreachable (<see cref="KafkaException"/>) at metadata time
/// is logged but does NOT fail host startup — transient broker issues
/// are the Kafka breaker's concern (R-KAFKA-BREAKER-01), not this
/// verifier's. Missing-topic drift IS a host startup concern (it is a
/// deployment misconfiguration, not a runtime condition).
/// </summary>
public sealed class TopicProvisioningHostedService : IHostedService
{
    private readonly string _kafkaBootstrapServers;
    private readonly bool _verifyOnStartup;
    private readonly bool _failIfMissing;
    private readonly TimeSpan _metadataTimeout;
    private readonly ILogger<TopicProvisioningHostedService>? _logger;

    public TopicProvisioningHostedService(
        string kafkaBootstrapServers,
        bool verifyOnStartup,
        bool failIfMissing,
        TimeSpan metadataTimeout,
        ILogger<TopicProvisioningHostedService>? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kafkaBootstrapServers);
        if (metadataTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(metadataTimeout), metadataTimeout,
                "metadataTimeout must be positive.");

        _kafkaBootstrapServers = kafkaBootstrapServers;
        _verifyOnStartup = verifyOnStartup;
        _failIfMissing = failIfMissing;
        _metadataTimeout = metadataTimeout;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_verifyOnStartup)
        {
            _logger?.LogInformation(
                "TopicProvisioningHostedService: VerifyTopicsOnStartup=false — skipping alignment check.");
            return;
        }

        var expected = KafkaCanonicalTopics.All;
        _logger?.LogInformation(
            "TopicProvisioningHostedService: verifying {Count} canonical topics against broker {Broker} (timeout {TimeoutMs}ms, failIfMissing={FailIfMissing}).",
            expected.Count, _kafkaBootstrapServers,
            (int)_metadataTimeout.TotalMilliseconds, _failIfMissing);

        var adminConfig = new AdminClientConfig { BootstrapServers = _kafkaBootstrapServers };
        using var admin = new AdminClientBuilder(adminConfig).Build();

        IReadOnlyList<string> missing;
        try
        {
            missing = await KafkaTopicVerifier.FindMissingTopicsAsync(
                admin, expected, _metadataTimeout, _logger, cancellationToken);
        }
        catch (KafkaException ex)
        {
            _logger?.LogError(ex,
                "TopicProvisioningHostedService: broker metadata call failed against {Broker}. " +
                "Alignment check skipped; host startup continues. " +
                "Check the R-KAFKA-BREAKER-01 posture once workers attach.",
                _kafkaBootstrapServers);
            return;
        }

        if (missing.Count == 0)
        {
            _logger?.LogInformation(
                "TopicProvisioningHostedService: alignment OK ({Count} topics present).",
                expected.Count);
            return;
        }

        foreach (var topic in missing)
        {
            _logger?.LogWarning(
                "TopicProvisioningHostedService: missing topic {Topic}.", topic);
        }

        _logger?.LogWarning(
            "TopicProvisioningHostedService: {MissingCount} of {ExpectedCount} canonical topics missing from broker {Broker}. " +
            "Run infrastructure/event-fabric/kafka/create-topics.sh to reconcile.",
            missing.Count, expected.Count, _kafkaBootstrapServers);

        if (_failIfMissing)
        {
            throw new InvalidOperationException(
                $"Topic alignment failed at startup: {missing.Count} canonical topic(s) missing from broker '{_kafkaBootstrapServers}'. " +
                $"Set Kafka:FailIfTopicsMissing=false to degrade to warn-only, " +
                $"or run infrastructure/event-fabric/kafka/create-topics.sh to reconcile. " +
                $"Missing: [{string.Join(", ", missing)}].");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
