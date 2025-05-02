namespace Loans.Servicing.Kafka.Events.CalculateFullLoanValue;

public record FullLoanValueCalculatedEvent(Guid ContractId, decimal FullLoanValue, Guid OperationId) : EventBase;