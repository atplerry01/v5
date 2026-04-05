using System.Diagnostics;
using Minio;
using Whyce.Shared.Contracts.Infrastructure.Health;

namespace Whyce.Platform.Host.Health;

public sealed class MinioHealthCheck : IHealthCheck
{
    private readonly IMinioClient _minioClient;

    public string Name => "minio";

    public MinioHealthCheck(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    public async Task<HealthCheckResult> CheckAsync()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await _minioClient.ListBucketsAsync();
            sw.Stop();
            return new HealthCheckResult(Name, true, "HEALTHY", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new HealthCheckResult(Name, false, "DOWN", sw.ElapsedMilliseconds, ex.Message);
        }
    }
}
