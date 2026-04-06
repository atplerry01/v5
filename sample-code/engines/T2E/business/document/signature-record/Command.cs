namespace Whycespace.Engines.T2E.Business.Document.SignatureRecord;

public record SignatureRecordCommand(
    string Action,
    string EntityId,
    object Payload
);
