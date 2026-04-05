using System.Diagnostics;
using Whyce.Shared.Contracts.Infrastructure.Health;

namespace Whyce.Platform.Host.Health;

public sealed class OpaHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly string _opaBaseUrl;

    public string Name => "opa";

    public OpaHealthCheck(HttpClient httpClient, string opaBaseUrl)
    {
        _httpClient = httpClient;
        _opaBaseUrl = opaBaseUrl.TrimEnd('/');
    }

    public async Task<HealthCheckResult> CheckAsync()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await _httpClient.GetAsync($"{_opaBaseUrl}/health");
            sw.Stop();

            if (response.IsSuccessStatusCode)
            {
                return new HealthCheckResult(Name, true, "HEALTHY", sw.ElapsedMilliseconds);
            }

            return new HealthCheckResult(
                Name, false, "DOWN", sw.ElapsedMilliseconds,
                $"HTTP {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new HealthCheckResult(Name, false, "DOWN", sw.ElapsedMilliseconds, ex.Message);
        }
    }
}
