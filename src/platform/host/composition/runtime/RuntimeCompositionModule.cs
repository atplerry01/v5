using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Composition.Abstractions;

namespace Whycespace.Platform.Host.Composition.Runtime;

public sealed class RuntimeCompositionModule : ICompositionModule
{
    public string Name => "runtime";

    public int Order => 1;

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRuntimeComposition(configuration);
    }
}
