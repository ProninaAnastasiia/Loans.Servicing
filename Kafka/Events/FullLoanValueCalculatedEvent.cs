namespace Loans.Servicing.Kafka.Events;

public record FullLoanValueCalculatedEvent(Guid ContractId, decimal FullLoanValue, Guid OperationId) : EventBase;