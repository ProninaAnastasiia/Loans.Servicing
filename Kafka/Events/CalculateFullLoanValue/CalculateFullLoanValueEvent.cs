namespace Loans.Servicing.Kafka.Events.CalculateFullLoanValue;

public record CalculateFullLoanValueEvent(
    Guid ContractId, decimal LoanAmount, int LoanTermMonths, decimal InterestRate,
    string PaymentType, decimal? InitialPaymentAmount, Guid OperationId) : EventBase;