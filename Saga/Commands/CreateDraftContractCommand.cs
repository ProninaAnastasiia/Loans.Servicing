namespace Loans.Servicing.Saga.Commands;

public class CreateDraftContractCommand
{
    public Guid ApplicationId { get; set; }
    Guid ClientId { get; }
    Guid DecisionId { get; }
    DateTime LodgementDate { get; }
    Guid CreditProductId { get; }
    decimal LoanAmount { get; }
    int LoanTermMonths { get; }
    decimal InterestRate { get; }
    string LoanPurpose { get; }
    string LoanType { get; }
    string PaymentType { get; }
    decimal? InitialPaymentAmount { get; }
}