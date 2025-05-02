namespace Loans.Servicing.Kafka.Events.CalculateContractValues;

public record CalculateContractValuesEvent(
    Guid ContractId, decimal LoanAmount, int LoanTermMonths, decimal InterestRate,
    string PaymentType, Guid OperationId) : EventBase;