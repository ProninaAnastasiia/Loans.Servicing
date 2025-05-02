namespace Loans.Servicing.Kafka.Events.CreateDraftContract;

public record CreateContractRequestedEvent(
    Guid ApplicationId, Guid ClientId, Guid DecisionId, DateTime LodgementDate,
    Guid CreditProductId, decimal LoanAmount, int LoanTermMonths,
    decimal InterestRate, string LoanPurpose, string LoanType, string PaymentType,
    decimal? InitialPaymentAmount, Guid OperationId) : EventBase;