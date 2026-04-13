namespace Whycespace.Shared.Contracts.Infrastructure.Health;

public interface IHealthCheck
{
    string Name { get; }
    Task<HealthCheckResult> CheckAsync();
}
