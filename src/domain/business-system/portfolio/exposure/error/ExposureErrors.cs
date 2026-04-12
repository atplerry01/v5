namespace Whycespace.Domain.BusinessSystem.Portfolio.Exposure;

public static class ExposureErrors
{
    public static ExposureDomainException MissingId()
        => new("ExposureId is required and must not be empty.");

    public static ExposureDomainException InvalidStateTransition(ExposureStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ExposureDomainException ContextRequired()
        => new("Exposure must define an exposure context.");

    public static ExposureDomainException LimitRequired()
        => new("Exposure must define a limit greater than zero.");

    public static ExposureDomainException LimitExceeded(decimal current, decimal limit)
        => new($"Exposure value '{current}' exceeds defined limit '{limit}'.");
}

public sealed class ExposureDomainException : Exception
{
    public ExposureDomainException(string message) : base(message) { }
}
