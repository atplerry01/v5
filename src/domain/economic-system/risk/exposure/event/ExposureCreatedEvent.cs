using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Risk.Exposure;

public sealed record ExposureCreatedEvent(
    ExposureId ExposureId,
    SourceId SourceId,
    ExposureType ExposureType,
    Amount TotalExposure,
    Currency Currency,
    Timestamp CreatedAt) : DomainEvent;
