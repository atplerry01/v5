using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Shared.Contracts.Infrastructure.Policy;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// Startup hardening — fires a single non-blocking warm-up request at OPA
/// so the first real policy evaluation does not pay the cold-start cost
/// (socket handshake, JIT, bundle load, rego compile cache).
///
/// Rules:
///   * MUST NOT block startup (StartAsync completes immediately)
///   * MUST NOT throw (warm-up is best-effort)
///   * Only reduces first-request latency; never affects correctness
/// </summary>
public sealed class OpaWarmupService : IHostedService
{
    private readonly OpaOptions _options;
    private readonly ILogger<OpaWarmupService>? _logger;

    public OpaWarmupService(OpaOptions options, ILogger<OpaWarmupService>? logger = null)
    {
        _options = options;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Fire-and-forget: warm-up must never block host bootstrap.
        _ = Task.Run(async () =>
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                var url = _options.Endpoint.TrimEnd('/') + "/v1/data/whyce/health";
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                };

                var response = await client.SendAsync(request, cancellationToken);

                _logger?.LogInformation(
                    "OPA warm-up completed with status {StatusCode}.",
                    (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger?.LogInformation(
                    "OPA warm-up failed (non-blocking): {Message}", ex.Message);
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
