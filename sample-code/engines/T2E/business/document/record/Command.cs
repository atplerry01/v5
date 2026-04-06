namespace Whycespace.Engines.T2E.Business.Document.Record;

public record RecordCommand(string Action, string EntityId, object Payload);
public sealed record CreateRecordCommand(string Id) : RecordCommand("Create", Id, null!);
