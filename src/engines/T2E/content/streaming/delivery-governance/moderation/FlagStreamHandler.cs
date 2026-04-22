using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Moderation;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Moderation;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.Moderation;

public sealed class FlagStreamHandler : IEngine
{
    private readonly IClock _clock;

    public FlagStreamHandler(IClock clock) => _clock = clock;

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FlagStreamCommand cmd) return Task.CompletedTask;
        var aggregate = ModerationAggregate.Flag(
            new ModerationId(cmd.ModerationId),
            new ModerationTargetRef(cmd.TargetId),
            cmd.FlagReason,
            new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
