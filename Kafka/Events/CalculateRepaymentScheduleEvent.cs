namespace Loans.Servicing.Kafka.Events;

public record CalculateRepaymentScheduleEvent(
    Guid ContractId, DateTime LodgementDate, decimal LoanAmount, int LoanTermMonths,
    decimal InterestRate, string PaymentType, decimal? InitialPaymentAmount, Guid OperationId) : EventBase;