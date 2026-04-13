using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Composition.Abstractions;

namespace Whycespace.Platform.Host.Composition.Projections;

public sealed class ProjectionCompositionModule : ICompositionModule
{
    public string Name => "projections";

    public int Order => 3;

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddProjectionComposition(configuration);
    }
}
