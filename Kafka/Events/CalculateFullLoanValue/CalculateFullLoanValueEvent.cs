namespace Loans.Servicing.Kafka.Events.CalculateFullLoanValue;

public record CalculateFullLoanValueEvent(decimal LoanAmount, int LoanTermMonths, decimal InterestRate,
    string PaymentType, Guid OperationId) : EventBase;