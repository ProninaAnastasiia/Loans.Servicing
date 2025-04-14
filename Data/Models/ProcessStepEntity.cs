using Loans.Servicing.Data.Enums;

namespace Loans.Servicing.Data.Models;

public class ProcessStepEntity
{
    public int Id { get; set; }
    public Guid ProcessId { get; set; }
    public int StepIndex { get; set; }
    public string StepName { get; set; } = null!;
    public StepStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}