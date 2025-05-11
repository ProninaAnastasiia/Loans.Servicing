namespace Loans.Servicing.Kafka.Events.CalculateRepaymentSchedule;

public record CalculateRepaymentScheduleEvent(
    Guid ContractId, decimal LoanAmount, int LoanTermMonths,
    decimal InterestRate, string PaymentType, Guid OperationId) : EventBase;