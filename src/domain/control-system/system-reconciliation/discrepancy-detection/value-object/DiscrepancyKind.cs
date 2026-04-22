namespace Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyDetection;

public enum DiscrepancyKind
{
    MissingRecord = 1,
    ExtraRecord = 2,
    ValueMismatch = 3,
    SequenceGap = 4,
    IntegrityViolation = 5
}
