namespace Loans.Servicing.Data.Dto;

public class CalculateAllContractValuesRequest
{
    public Guid ContractId { get; set; }

    public decimal LoanAmount { get; set; }

    public int LoanTermMonths { get; set; }

    public decimal InterestRate { get; set; }

    public string PaymentType { get; set; }
}