using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Composition.Abstractions;

namespace Whycespace.Platform.Host.Composition.Core;

public sealed class CoreCompositionModule : ICompositionModule
{
    public string Name => "core";

    public int Order => 0;

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCoreComposition(configuration);
    }
}
