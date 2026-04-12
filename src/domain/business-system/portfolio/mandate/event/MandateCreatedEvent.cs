namespace Whycespace.Domain.BusinessSystem.Portfolio.Mandate;

public sealed record MandateCreatedEvent(
    MandateId MandateId,
    MandateName MandateName);
