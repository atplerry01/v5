using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Platform.Host.Composition.Abstractions;

namespace Whyce.Platform.Host.Composition.Runtime;

public sealed class RuntimeCompositionModule : ICompositionModule
{
    public string Name => "runtime";

    public int Order => 1;

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRuntimeComposition(configuration);
    }
}
