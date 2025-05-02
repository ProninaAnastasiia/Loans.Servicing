namespace Loans.Servicing.Kafka.Events.CalculateContractValues;

public record ContractValuesUpdatedEvent(Guid ContractId, Guid ScheduleId, decimal MonthlyPaymentAmount,
    decimal TotalPaymentAmount,decimal TotalInterestPaid, decimal FullLoanValue, Guid OperationId) : EventBase;