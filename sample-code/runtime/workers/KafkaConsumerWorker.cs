using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Shared.Contracts.Messaging;

namespace Whycespace.Runtime.Workers;

/// <summary>
/// Domain-separated consumer worker.
/// Each domain context gets its own consumer group for independent scaling
/// and no cross-domain blocking. Follows WBSM v3 consumer group topology.
/// No direct Kafka dependency — uses IMessageConsumer contract.
/// </summary>
public class KafkaConsumerWorker : BackgroundService
{
    private readonly ILogger<KafkaConsumerWorker> _logger;
    private readonly IMessageConsumerFactory _consumerFactory;

    private static readonly Dictionary<string, string[]> DomainTopicGroups = new()
    {
        ["economic"] = [
            "whyce.economic.capital.wallet.events"
        ],
        ["identity"] = [
            "whyce.identity.access.identity.events"
        ],
        ["workflow"] = [
            "whyce.execution.workflow.lifecycle.events"
        ]
    };

    public KafkaConsumerWorker(
        ILogger<KafkaConsumerWorker> logger,
        IMessageConsumerFactory consumerFactory)
    {
        _logger = logger;
        _consumerFactory = consumerFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();

        foreach (var (domain, topics) in DomainTopicGroups)
        {
            tasks.Add(RunDomainConsumerAsync(domain, topics, stoppingToken));
        }

        _logger.LogInformation(
            "KafkaConsumerWorker started — {Count} domain consumer groups: {Domains}",
            DomainTopicGroups.Count, string.Join(", ", DomainTopicGroups.Keys));

        await Task.WhenAll(tasks);
    }

    private async Task RunDomainConsumerAsync(
        string domain, string[] topics, CancellationToken stoppingToken)
    {
        var groupId = $"whyce-consumer-{domain}";
        await using var consumer = _consumerFactory.Create(groupId);
        consumer.Subscribe(topics);

        _logger.LogInformation(
            "Consumer group '{GroupId}' subscribed to {Topics}",
            groupId, string.Join(", ", topics));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = await consumer.ConsumeAsync(TimeSpan.FromSeconds(1), stoppingToken);
                if (message is null) continue;

                _logger.LogInformation(
                    "[{Domain}] Consumed {Topic}: {Key}",
                    domain, message.Topic, message.Key);

                consumer.Commit();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Domain}] Consumer error", domain);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("Consumer group '{GroupId}' stopped", groupId);
    }
}
