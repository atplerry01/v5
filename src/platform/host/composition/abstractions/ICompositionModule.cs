using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Whycespace.Platform.Host.Composition.Abstractions;

/// <summary>
/// Composition module contract. Modules orchestrate registration only —
/// they delegate to the existing category Add*Composition extensions and
/// contain no logic of their own. Order is deterministic and explicit;
/// no reflection-based discovery is permitted.
/// </summary>
public interface ICompositionModule
{
    string Name { get; }

    int Order { get; }

    void Register(IServiceCollection services, IConfiguration configuration);
}
