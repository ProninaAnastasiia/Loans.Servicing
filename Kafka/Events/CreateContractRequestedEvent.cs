namespace Loans.Servicing.Kafka.Events;

public record CreateContractRequestedEvent : EventBase
{
    public Guid ApplicationId { get; init; }
    public Guid ClientId { get; init; }
    public Guid DecisionId { get; init; }
    public DateTime LodgementDate { get; init; }
    public Guid CreditProductId { get; init; }
    public decimal LoanAmount { get; init; }
    public int LoanTermMonths { get; init; }
    public decimal InterestRate { get; init; }
    public string LoanPurpose { get; init; }
    public string LoanType { get; init; }
    public string PaymentType { get; init; }
    public decimal? InitialPaymentAmount { get; init; }
    public Guid OperationId { get; init; }
}