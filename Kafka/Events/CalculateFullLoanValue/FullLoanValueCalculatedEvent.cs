namespace Loans.Servicing.Kafka.Events.CalculateFullLoanValue;

public record FullLoanValueCalculatedEvent(decimal FullLoanValue, Guid OperationId) : EventBase;