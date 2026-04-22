using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Trust.Identity.Credential;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Trust.Identity.Credential;

namespace Whycespace.Platform.Host.Composition.Trust.Identity.Credential;

public static class CredentialApplicationModule
{
    public static IServiceCollection AddCredentialApplication(this IServiceCollection services)
    {
        services.AddTransient<IssueCredentialHandler>();
        services.AddTransient<ActivateCredentialHandler>();
        services.AddTransient<RevokeCredentialHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<IssueCredentialCommand, IssueCredentialHandler>();
        engine.Register<ActivateCredentialCommand, ActivateCredentialHandler>();
        engine.Register<RevokeCredentialCommand, RevokeCredentialHandler>();
    }
}
