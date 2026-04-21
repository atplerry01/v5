using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Humancapital.Participant;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Participant;

namespace Whycespace.Platform.Host.Composition.Structural.Humancapital.Participant.Application;

public static class ParticipantApplicationModule
{
    public static IServiceCollection AddParticipantApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterParticipantHandler>();
        services.AddTransient<PlaceParticipantHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterParticipantCommand, RegisterParticipantHandler>();
        engine.Register<PlaceParticipantCommand, PlaceParticipantHandler>();
    }
}
