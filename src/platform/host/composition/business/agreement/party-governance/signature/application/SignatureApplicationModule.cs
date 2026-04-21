using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Agreement.PartyGovernance.Signature;
using Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Signature;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Agreement.PartyGovernance.Signature.Application;

public static class SignatureApplicationModule
{
    public static IServiceCollection AddSignatureApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateSignatureHandler>();
        services.AddTransient<SignSignatureHandler>();
        services.AddTransient<RevokeSignatureHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateSignatureCommand, CreateSignatureHandler>();
        engine.Register<SignSignatureCommand, SignSignatureHandler>();
        engine.Register<RevokeSignatureCommand, RevokeSignatureHandler>();
    }
}
