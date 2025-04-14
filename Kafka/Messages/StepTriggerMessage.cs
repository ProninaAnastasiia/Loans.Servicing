namespace Loans.Servicing.Kafka.Messages;

public class StepTriggerMessage
{
    public Guid ProcessId { get; set; }
    public int StepIndex { get; set; }
}