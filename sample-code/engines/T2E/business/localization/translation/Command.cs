namespace Whycespace.Engines.T2E.Business.Localization.Translation;

public record TranslationCommand(
    string Action,
    string EntityId,
    object Payload
);
