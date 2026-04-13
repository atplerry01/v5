using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Composition.Abstractions;

namespace Whycespace.Platform.Host.Composition.Observability;

public sealed class ObservabilityCompositionModule : ICompositionModule
{
    public string Name => "observability";

    public int Order => 4;

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddObservabilityComposition(configuration);
    }
}
