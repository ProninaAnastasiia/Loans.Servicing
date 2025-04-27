namespace Loans.Servicing.Kafka.Events;

public record DraftContractCreatedEvent(
    string ContractId, string ApplicationId, string ClientId, string DecisionId, DateTime LodgementDate,
    string CreditProductId, decimal LoanAmount, int LoanTermMonths,
    decimal InterestRate, string LoanPurpose, string LoanType, string PaymentType,
    decimal? InitialPaymentAmount, string OperationId) : EventBase;