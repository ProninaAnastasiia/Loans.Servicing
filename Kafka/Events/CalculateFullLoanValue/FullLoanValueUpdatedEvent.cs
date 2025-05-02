namespace Loans.Servicing.Kafka.Events.CalculateFullLoanValue;

public record FullLoanValueUpdatedEvent(Guid ContractId, decimal FullLoanValue, Guid OperationId) : EventBase;