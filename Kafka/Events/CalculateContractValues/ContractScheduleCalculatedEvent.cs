namespace Loans.Servicing.Kafka.Events.CalculateContractValues;

public record ContractScheduleCalculatedEvent(Guid ContractId, Guid ScheduleId, Guid OperationId) : EventBase;