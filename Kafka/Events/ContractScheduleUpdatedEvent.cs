namespace Loans.Servicing.Kafka.Events;

public record ContractScheduleUpdatedEvent(Guid ContractId, Guid ScheduleId, Guid OperationId) : EventBase;