using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Envelope.MessageEnvelope;
using Whycespace.Shared.Contracts.Platform.Envelope.MessageEnvelope;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Envelope.MessageEnvelope.Application;

public static class MessageEnvelopeApplicationModule
{
    public static IServiceCollection AddMessageEnvelopeApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateMessageEnvelopeHandler>();
        services.AddTransient<DispatchMessageEnvelopeHandler>();
        services.AddTransient<AcknowledgeMessageEnvelopeHandler>();
        services.AddTransient<RejectMessageEnvelopeHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateMessageEnvelopeCommand, CreateMessageEnvelopeHandler>();
        engine.Register<DispatchMessageEnvelopeCommand, DispatchMessageEnvelopeHandler>();
        engine.Register<AcknowledgeMessageEnvelopeCommand, AcknowledgeMessageEnvelopeHandler>();
        engine.Register<RejectMessageEnvelopeCommand, RejectMessageEnvelopeHandler>();
    }
}
