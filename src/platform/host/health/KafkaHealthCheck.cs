using System.Diagnostics;
using Confluent.Kafka;
using Whyce.Shared.Contracts.Infrastructure.Health;

namespace Whyce.Platform.Host.Health;

public sealed class KafkaHealthCheck : IHealthCheck
{
    private readonly string _bootstrapServers;

    public string Name => "kafka";

    public KafkaHealthCheck(string bootstrapServers)
    {
        _bootstrapServers = bootstrapServers;
    }

    public async Task<HealthCheckResult> CheckAsync()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var adminClient = new AdminClientBuilder(
                new AdminClientConfig { BootstrapServers = _bootstrapServers })
                .Build();

            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

            sw.Stop();

            if (metadata.Brokers.Count == 0)
            {
                return new HealthCheckResult(Name, false, "DOWN", sw.ElapsedMilliseconds, "No brokers available");
            }

            return new HealthCheckResult(Name, true, "HEALTHY", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new HealthCheckResult(Name, false, "DOWN", sw.ElapsedMilliseconds, ex.Message);
        }
    }
}
