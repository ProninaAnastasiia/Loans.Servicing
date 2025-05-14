namespace Loans.Servicing.Kafka.Events.InnerEvents;

public record CalculateAllValuesRequested(
    Guid ContractId, decimal LoanAmount, int LoanTermMonths,
    decimal InterestRate, string PaymentType, Guid OperationId) : EventBase;