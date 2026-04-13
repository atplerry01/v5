using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Composition.Abstractions;

namespace Whycespace.Platform.Host.Composition.Infrastructure;

public sealed class InfrastructureCompositionModule : ICompositionModule
{
    public string Name => "infrastructure";

    public int Order => 2;

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructureComposition(configuration);
    }
}
