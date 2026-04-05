using System.Diagnostics;
using Whyce.Shared.Contracts.Infrastructure.Health;

namespace Whyce.Platform.Host.Health;

public sealed class RuntimeHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider;

    public string Name => "runtime";

    public RuntimeHealthCheck(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<HealthCheckResult> CheckAsync()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var isInitialized = _serviceProvider is not null;
            sw.Stop();

            if (!isInitialized)
            {
                return new HealthCheckResult(Name, false, "DOWN", sw.ElapsedMilliseconds, "Runtime not initialized");
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
