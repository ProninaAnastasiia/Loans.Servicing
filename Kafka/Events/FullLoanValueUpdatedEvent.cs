namespace Loans.Servicing.Kafka.Events;

public record FullLoanValueUpdatedEvent(Guid ContractId, decimal FullLoanValue, Guid OperationId) : EventBase;