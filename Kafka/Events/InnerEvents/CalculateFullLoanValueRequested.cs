namespace Loans.Servicing.Kafka.Events.InnerEvents;

public record CalculateFullLoanValueRequested( decimal LoanAmount, int LoanTermMonths, decimal InterestRate,
    string PaymentType, Guid OperationId) : EventBase;