using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Platform.Host.Composition.Abstractions;

namespace Whyce.Platform.Host.Composition.Infrastructure;

public sealed class InfrastructureCompositionModule : ICompositionModule
{
    public string Name => "infrastructure";

    public int Order => 2;

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructureComposition(configuration);
    }
}
