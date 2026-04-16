using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Call;

public sealed class CallSpecification : Specification<CallStatus>
{
    private static readonly IReadOnlyDictionary<CallStatus, IReadOnlySet<CallStatus>> Allowed =
        new Dictionary<CallStatus, IReadOnlySet<CallStatus>>
        {
            [CallStatus.Initiated] = new HashSet<CallStatus> { CallStatus.Ringing, CallStatus.Rejected, CallStatus.Ended },
            [CallStatus.Ringing] = new HashSet<CallStatus> { CallStatus.Answered, CallStatus.Rejected, CallStatus.Ended },
            [CallStatus.Answered] = new HashSet<CallStatus> { CallStatus.Ended },
            [CallStatus.Rejected] = new HashSet<CallStatus>(),
            [CallStatus.Ended] = new HashSet<CallStatus>()
        };

    public override bool IsSatisfiedBy(CallStatus entity) =>
        entity == CallStatus.Initiated || entity == CallStatus.Ringing || entity == CallStatus.Answered;

    public void EnsureTransition(CallStatus from, CallStatus to)
    {
        if (!Allowed.TryGetValue(from, out var set) || !set.Contains(to))
            throw CallErrors.InvalidTransition(from, to);
    }
}
