namespace Loans.Servicing.Data.Dto;

public class CalculateFullLoanValueRequest
{
    public decimal LoanAmount { get; set; }

    public int LoanTermMonths { get; set; }

    public decimal InterestRate { get; set; }

    public string PaymentType { get; set; }
}