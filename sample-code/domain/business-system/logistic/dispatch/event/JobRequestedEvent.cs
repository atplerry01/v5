using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Logistic.Dispatch;

public sealed record JobRequestedEvent(
    Guid JobId,
    Guid OperatorId,
    double StartLatitude,
    double StartLongitude,
    string StartLabel,
    double EndLatitude,
    double EndLongitude,
    string EndLabel
) : DomainEvent;
