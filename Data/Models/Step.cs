namespace Loans.Servicing.Data.Models;

public class Step
{
    public Guid StepId  { get; set; }
    public string Description { get; set; }
    public string Result { get; set; }
    public string ErrorMessage { get; set; }
}