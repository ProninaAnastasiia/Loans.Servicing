namespace Loans.Servicing.Kafka.Events.CalculateContractValues;

public record ContractScheduleUpdatedEvent(Guid ContractId, Guid ScheduleId, Guid OperationId) : EventBase;