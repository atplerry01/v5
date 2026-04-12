namespace Whycespace.Domain.BusinessSystem.Portfolio.Exposure;

public sealed record ExposureCreatedEvent(
    ExposureId ExposureId,
    ExposureContext ExposureContext,
    ExposureLimit Limit);
