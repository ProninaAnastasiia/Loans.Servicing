namespace Loans.Servicing.Process;

public class Step
{
    public int StepIndex { get; set; }
    public string Status { get; set; } // Например: "Pending", "InProgress", "Completed", "Failed"
    public string Description { get; set; }
}