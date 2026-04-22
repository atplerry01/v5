namespace Whycespace.Domain.ControlSystem.Scheduling.ExecutionControl;

public enum ControlSignal { Start = 1, Stop = 2, Suspend = 3, Resume = 4 }
public enum ControlSignalOutcome { Acknowledged = 1, Applied = 2, Rejected = 3 }
