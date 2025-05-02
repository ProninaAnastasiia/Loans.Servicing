namespace Loans.Servicing.Kafka.Events.CalculateContractValues;

public record ContractValuesCalculatedEvent(Guid ContractId, decimal MonthlyPaymentAmount,
    decimal TotalPaymentAmount,decimal TotalInterestPaid, decimal FullLoanValue, Guid OperationId) : EventBase;