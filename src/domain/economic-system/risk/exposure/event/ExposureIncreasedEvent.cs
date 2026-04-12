using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Risk.Exposure;

public sealed record ExposureIncreasedEvent(
    ExposureId ExposureId,
    Amount IncreasedBy,
    Amount NewTotal) : DomainEvent;
