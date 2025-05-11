namespace Loans.Servicing.Kafka.Events.InnerEvents;

public record CalculateScheduleRequested(
    Guid ContractId, decimal LoanAmount, int LoanTermMonths,
    decimal InterestRate, string PaymentType, Guid OperationId) : EventBase;