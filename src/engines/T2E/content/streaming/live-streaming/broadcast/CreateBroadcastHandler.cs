using Whycespace.Domain.ContentSystem.Invariant.BroadcastStreamBinding;
using Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Broadcast;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Stream;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.LiveStreaming.Broadcast;

public sealed class CreateBroadcastHandler : IEngine
{
    private readonly BroadcastStreamBindingPolicy _policy;
    private readonly IStreamStatusLookup _streamLookup;

    public CreateBroadcastHandler(BroadcastStreamBindingPolicy policy, IStreamStatusLookup streamLookup)
    {
        _policy = policy;
        _streamLookup = streamLookup;
    }

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateBroadcastCommand cmd) return;

        // Cross-system invariant — broadcast-stream-binding. Evaluate BEFORE
        // aggregate mutation per
        // claude/templates/delivery-pattern/02-engine-skeleton.md
        // § Cross-System Invariants. A denied binding throws a
        // DomainException so the command surfaces as a failure and no
        // event is emitted.
        var snapshot = await _streamLookup.GetAsync(cmd.StreamId);
        var decision = _policy.Evaluate(new BroadcastStreamBindingInput(
            StreamId: cmd.StreamId,
            StreamExists: snapshot.Exists,
            StreamStatusIsTerminal: snapshot.IsTerminal));

        if (decision.IsDenied)
            throw new DomainInvariantViolationException(
                $"BroadcastStreamBindingPolicy denied create: {decision.Reason}.");

        var aggregate = BroadcastAggregate.Create(
            new BroadcastId(cmd.BroadcastId),
            new StreamRef(cmd.StreamId),
            new Timestamp(cmd.CreatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
