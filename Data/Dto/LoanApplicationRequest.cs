namespace Loans.Servicing.Data.Dto;

public class LoanApplicationRequest
{
    public string ApplicationId { get; set; }

    public string ClientId { get; set; }

    public string DecisionId { get; set; }

    public DateTime LodgementDate { get; set; }

    public string CreditProductId { get; set; }

    public decimal LoanAmount { get; set; }

    public int LoanTermMonths { get; set; }

    public decimal InterestRate { get; set; }

    public string LoanPurpose { get; set; }

    public string LoanType { get; set; }

    public string PaymentType { get; set; }

    public decimal? InitialPaymentAmount { get; set; }
}