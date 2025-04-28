namespace Loans.Servicing.Kafka.Events;

public record RepaymentScheduleCalculatedEvent(Guid ContractId, Guid ScheduleId, Guid OperationId) : EventBase;