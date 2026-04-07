using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Platform.Host.Composition.Abstractions;

namespace Whyce.Platform.Host.Composition.Core;

public sealed class CoreCompositionModule : ICompositionModule
{
    public string Name => "core";

    public int Order => 0;

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCoreComposition(configuration);
    }
}
