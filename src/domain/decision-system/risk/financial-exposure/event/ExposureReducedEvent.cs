using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.DecisionSystem.Risk.Exposure;

public sealed record ExposureReducedEvent(
    ExposureId ExposureId,
    Amount ReducedBy,
    Amount NewTotal) : DomainEvent;
