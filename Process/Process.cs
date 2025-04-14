namespace Loans.Servicing.Process;

public class Process
{
    public Guid Id { get; set; }
    public string Status { get; set; }  // Например: "InProgress", "Completed", "Failed"
    public List<Step> Steps { get; set; } = new List<Step>();
}