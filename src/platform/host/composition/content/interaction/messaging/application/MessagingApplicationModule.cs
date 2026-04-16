using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Interaction.Messaging;
using Whycespace.Shared.Contracts.Content.Interaction.Messaging;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Interaction.Messaging.Application;

public static class MessagingApplicationModule
{
    public static IServiceCollection AddMessagingApplication(this IServiceCollection services)
    {
        services.AddTransient<SendMessageHandler>();
        services.AddTransient<MarkMessageDeliveredHandler>();
        services.AddTransient<MarkMessageReadHandler>();
        services.AddTransient<EditMessageHandler>();
        services.AddTransient<RetractMessageHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<SendMessageCommand, SendMessageHandler>();
        engine.Register<MarkMessageDeliveredCommand, MarkMessageDeliveredHandler>();
        engine.Register<MarkMessageReadCommand, MarkMessageReadHandler>();
        engine.Register<EditMessageCommand, EditMessageHandler>();
        engine.Register<RetractMessageCommand, RetractMessageHandler>();
    }
}
